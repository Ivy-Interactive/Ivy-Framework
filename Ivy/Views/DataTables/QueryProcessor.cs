using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ArrowField = Apache.Arrow.Field;
using SystemType = System.Type;

namespace Ivy.Views.DataTables;

public class QueryResult
{
    public byte[] ArrowData { get; set; } = [];
    public int Offset { get; set; }
    public int RowCount { get; set; }
    public int TotalRows { get; set; }
}

public class ValuesResult
{
    public List<string> Values { get; set; } = new();
    public int TotalValues { get; set; }
}

public class QueryProcessor(ILogger<QueryProcessor>? logger = null, IDistributedCache? cache = null)
{
    public QueryResult ProcessQuery(IQueryable queryable, DataTableQuery query, Func<object, object?>? idSelector = null)
    {
        try
        {
            logger?.LogInformation("Processing query with filter: {HasFilter}", query.Filter != null);

            // Generate cache key if caching is enabled
            string? cacheKey = null;
            if (cache != null)
            {
                cacheKey = GenerateCacheKey("Query", queryable.ElementType.FullName, query);
                logger?.LogDebug("Generated cache key: {CacheKey}", cacheKey);

                // Try to get from cache
                var cachedData = cache.Get(cacheKey);
                if (cachedData != null)
                {
                    logger?.LogInformation("Cache hit for query");
                    return DeserializeQueryResult(cachedData);
                }
                logger?.LogDebug("Cache miss for query");
            }

            var processedQuery = queryable;

            // Apply filtering
            if (query.Filter != null)
            {
                logger?.LogDebug("Applying filter");
                processedQuery = ApplyFilter(processedQuery, query.Filter);
                logger?.LogDebug("Filter applied successfully");
            }

            // Apply sorting
            if (query.Sort.Any())
            {
                processedQuery = ApplySort(processedQuery, query.Sort);
            }

            // Get total count before pagination
            var totalRows = processedQuery.Cast<object>().Count();
            logger?.LogDebug("Total rows before pagination: {TotalRows}", totalRows);

            // Apply pagination
            if (query.Offset > 0)
            {
                var skipMethod = typeof(Queryable).GetMethods()
                    .FirstOrDefault(m => m.Name == "Skip" && m.GetParameters().Length == 2)?
                    .MakeGenericMethod(queryable.ElementType);

                if (skipMethod != null)
                {
                    processedQuery = (IQueryable)skipMethod.Invoke(null, new object[] { processedQuery, query.Offset })!;
                }
            }

            // Apply limit - always apply if specified, even if 0
            var takeMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Take" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(queryable.ElementType);

            if (takeMethod != null)
            {
                processedQuery = (IQueryable)takeMethod.Invoke(null, new object[] { processedQuery, query.Limit })!;
            }

            // Execute query and get results
            logger?.LogDebug("Executing query");
            var results = processedQuery.Cast<object>().ToList();
            logger?.LogInformation("Query executed, got {ResultCount} results", results.Count);

            // Convert to Arrow table
            logger?.LogDebug("Converting to Arrow table");
            var arrowData = ConvertToArrowTable(results, query.SelectColumns, queryable.ElementType, idSelector);
            logger?.LogInformation("Arrow conversion complete, {ByteCount} bytes", arrowData.Length);

            var result = new QueryResult
            {
                ArrowData = arrowData,
                Offset = query.Offset,
                RowCount = results.Count,
                TotalRows = totalRows
            };

            // Store in cache if enabled
            if (cache != null && cacheKey != null)
            {
                try
                {
                    var serialized = SerializeQueryResult(result);
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };
                    cache.Set(cacheKey, serialized, cacheOptions);
                    logger?.LogDebug("Stored query result in cache");
                }
                catch (Exception cacheEx)
                {
                    logger?.LogWarning(cacheEx, "Failed to cache query result");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error processing query");
            throw;
        }
    }

    private IQueryable ApplySort(IQueryable query, IEnumerable<SortOrder> sortOrders)
    {
        var sortOrdersList = sortOrders.ToList();
        if (!sortOrdersList.Any())
            return query;

        var elementType = query.ElementType;
        var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "x");

        for (int i = 0; i < sortOrdersList.Count; i++)
        {
            var sortOrder = sortOrdersList[i];
            var propertyInfo = elementType.GetProperty(sortOrder.Column);

            if (propertyInfo == null)
                continue;

            System.Linq.Expressions.Expression sortExpression;
            SystemType sortType;

            // Special handling for string arrays - sort by first element
            if (propertyInfo.PropertyType.IsArray && propertyInfo.PropertyType.GetElementType() == typeof(string))
            {
                logger?.LogDebug("Sorting string array column by first element: {Column}", sortOrder.Column);

                var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);

                // Create expression: array != null && array.Length > 0 ? array[0] : null
                // This gets the first element or null if empty
                var nullCheck = System.Linq.Expressions.Expression.NotEqual(
                    property,
                    System.Linq.Expressions.Expression.Constant(null, propertyInfo.PropertyType)
                );

                var lengthProperty = System.Linq.Expressions.Expression.Property(property, "Length");
                var lengthCheck = System.Linq.Expressions.Expression.GreaterThan(
                    lengthProperty,
                    System.Linq.Expressions.Expression.Constant(0)
                );

                var hasElements = System.Linq.Expressions.Expression.AndAlso(nullCheck, lengthCheck);

                // array[0]
                var firstElement = System.Linq.Expressions.Expression.ArrayIndex(
                    property,
                    System.Linq.Expressions.Expression.Constant(0)
                );

                // hasElements ? array[0] : null
                sortExpression = System.Linq.Expressions.Expression.Condition(
                    hasElements,
                    firstElement,
                    System.Linq.Expressions.Expression.Constant(null, typeof(string))
                );

                sortType = typeof(string);
            }
            else
            {
                sortExpression = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);
                sortType = propertyInfo.PropertyType;
            }

            var lambda = System.Linq.Expressions.Expression.Lambda(sortExpression, parameter);

            // For the first sort, use OrderBy/OrderByDescending
            // For subsequent sorts, use ThenBy/ThenByDescending
            string methodName;
            if (i == 0)
            {
                methodName = sortOrder.Direction == Ivy.Protos.DataTable.SortDirection.Asc ? "OrderBy" : "OrderByDescending";
            }
            else
            {
                methodName = sortOrder.Direction == Ivy.Protos.DataTable.SortDirection.Asc ? "ThenBy" : "ThenByDescending";
            }

            var method = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2)?
                .MakeGenericMethod(elementType, sortType);

            if (method != null)
            {
                query = (IQueryable)method.Invoke(null, new object[] { query, lambda })!;
            }
        }

        return query;
    }

    private IQueryable ApplyFilter(IQueryable query, Filter filter)
    {
        try
        {
            logger?.LogDebug("Starting filter application for type {ElementType}", query.ElementType.Name);

            var elementType = query.ElementType;
            var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "x");

            logger?.LogDebug("Building filter expression");
            var predicate = BuildFilterExpression(filter, parameter, elementType);

            if (predicate == null)
            {
                logger?.LogDebug("No predicate generated, returning original query");
                return query;
            }

            logger?.LogDebug("Creating lambda expression");
            var lambda = System.Linq.Expressions.Expression.Lambda(predicate, parameter);

            logger?.LogDebug("Getting Where method");
            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(elementType);

            if (whereMethod != null)
            {
                logger?.LogDebug("Invoking Where method");
                query = (IQueryable)whereMethod.Invoke(null, [query, lambda])!;
                logger?.LogDebug("Filter applied successfully");
            }
            else
            {
                logger?.LogWarning("Could not find Where method");
            }

            return query;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error applying filter");
            throw;
        }
    }

    private System.Linq.Expressions.Expression? BuildFilterExpression(Filter filter, System.Linq.Expressions.ParameterExpression parameter, SystemType elementType)
    {
        System.Linq.Expressions.Expression? expression = null;

        if (filter.Condition != null)
        {
            expression = BuildConditionExpression(filter.Condition, parameter, elementType);
        }
        else if (filter.Group != null)
        {
            expression = BuildGroupExpression(filter.Group, parameter, elementType);
        }

        // Apply negation if specified
        if (expression != null && filter.Negate)
        {
            expression = System.Linq.Expressions.Expression.Not(expression);
        }

        return expression;
    }

    private System.Linq.Expressions.Expression? BuildConditionExpression(Condition condition, System.Linq.Expressions.ParameterExpression parameter, SystemType elementType)
    {
        var propertyInfo = elementType.GetProperty(condition.Column);
        if (propertyInfo == null)
            throw new InvalidOperationException($"Column '{condition.Column}' not found in type {elementType.Name}");

        var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);

        return condition.Function.ToLowerInvariant() switch
        {
            "contains" => BuildContainsExpression(property, condition.Args),
            "notcontains" => BuildNotContainsExpression(property, condition.Args),
            "equals" => BuildEqualsExpression(property, condition.Args),
            "notequals" => BuildNotEqualsExpression(property, condition.Args),
            "greaterthan" => BuildGreaterThanExpression(property, condition.Args),
            "greaterthanorequal" => BuildGreaterThanOrEqualExpression(property, condition.Args),
            "lessthan" => BuildLessThanExpression(property, condition.Args),
            "lessthanorequal" => BuildLessThanOrEqualExpression(property, condition.Args),
            "startswith" => BuildStartsWithExpression(property, condition.Args),
            "endswith" => BuildEndsWithExpression(property, condition.Args),
            "blank" => BuildBlankExpression(property),
            "notblank" => BuildNotBlankExpression(property),
            "inrange" => BuildInRangeExpression(property, condition.Args),
            "before" => BuildLessThanExpression(property, condition.Args),
            "after" => BuildGreaterThanExpression(property, condition.Args),
            _ => throw new NotSupportedException($"Filter function '{condition.Function}' is not supported")
        };
    }

    private System.Linq.Expressions.Expression? BuildGroupExpression(FilterGroup group, System.Linq.Expressions.ParameterExpression parameter, SystemType elementType)
    {
        var expressions = new List<System.Linq.Expressions.Expression>();

        foreach (var childFilter in group.Filters)
        {
            var childExpression = BuildFilterExpression(childFilter, parameter, elementType);
            if (childExpression != null)
                expressions.Add(childExpression);
        }

        if (!expressions.Any())
            return null;

        // Combine expressions with AND or OR
        var result = expressions.First();
        for (int i = 1; i < expressions.Count; i++)
        {
            result = group.Op == FilterGroup.Types.LogicalOperator.And
                ? System.Linq.Expressions.Expression.AndAlso(result, expressions[i])
                : System.Linq.Expressions.Expression.OrElse(result, expressions[i]);
        }

        return result;
    }

    private System.Linq.Expressions.Expression? BuildContainsExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        try
        {
            logger?.LogDebug("Building contains expression for property {PropertyName} of type {PropertyType}", property.Member.Name, property.Type);

            var arg = args.FirstOrDefault();
            if (arg == null)
            {
                logger?.LogDebug("No arguments provided for contains expression");
                return null;
            }

            // Extract the string value from the protobuf Any
            logger?.LogDebug("Extracting string value from protobuf Any");
            var searchValue = ExtractStringValue(arg);
            if (searchValue == null)
            {
                logger?.LogWarning("Failed to extract search value for contains expression");
                return null;
            }

            logger?.LogDebug("Search value: '{SearchValue}'", searchValue);

            // Use case-insensitive Contains method
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            if (containsMethod == null)
            {
                logger?.LogWarning("Could not find Contains method with StringComparison");
                return null;
            }

            var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
            var comparisonExpression = System.Linq.Expressions.Expression.Constant(StringComparison.OrdinalIgnoreCase);

            // Handle nullable properties
            if (property.Type == typeof(string))
            {
                logger?.LogDebug("Creating case-insensitive string contains expression");

                // Need to handle null strings - use null-conditional approach
                var nullCheck = System.Linq.Expressions.Expression.NotEqual(
                    property,
                    System.Linq.Expressions.Expression.Constant(null, typeof(string))
                );

                var containsCall = System.Linq.Expressions.Expression.Call(
                    property,
                    containsMethod,
                    searchValueExpression,
                    comparisonExpression
                );

                // Combine null check with contains: property != null && property.Contains(searchValue, OrdinalIgnoreCase)
                return System.Linq.Expressions.Expression.AndAlso(nullCheck, containsCall);
            }
            else
            {
                logger?.LogDebug("Converting non-string property to string first");
                // Convert to string first, then apply case-insensitive contains
                var toStringMethod = property.Type.GetMethod("ToString", System.Type.EmptyTypes);
                if (toStringMethod != null)
                {
                    var toStringCall = System.Linq.Expressions.Expression.Call(property, toStringMethod);

                    // Check for null after ToString (though ToString rarely returns null)
                    var nullCheck = System.Linq.Expressions.Expression.NotEqual(
                        toStringCall,
                        System.Linq.Expressions.Expression.Constant(null, typeof(string))
                    );

                    var containsCall = System.Linq.Expressions.Expression.Call(
                        toStringCall,
                        containsMethod,
                        searchValueExpression,
                        comparisonExpression
                    );

                    return System.Linq.Expressions.Expression.AndAlso(nullCheck, containsCall);
                }
                else
                {
                    logger?.LogWarning("Could not find ToString method for type {PropertyType}", property.Type);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error building contains expression");
            throw;
        }
    }

    private System.Linq.Expressions.Expression? BuildEqualsExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        // Special handling for string arrays - "equals" means "contains"
        if (property.Type.IsArray && property.Type.GetElementType() == typeof(string))
        {
            var searchValue = ExtractStringValue(arg);
            if (searchValue == null) return null;

            // Use Array.Contains to check if the array contains the value
            var containsMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(typeof(string));

            if (containsMethod == null) return null;

            var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);

            // Enumerable.Contains(array, value)
            return System.Linq.Expressions.Expression.Call(
                containsMethod,
                property,
                searchValueExpression
            );
        }

        var value = ExtractTypedValue(arg, property.Type);

        // Handle null comparison
        if (value == null || (value is string str && string.IsNullOrEmpty(str)))
        {
            // For null/empty string, return property == null
            return System.Linq.Expressions.Expression.Equal(
                property,
                System.Linq.Expressions.Expression.Constant(null, property.Type)
            );
        }

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.Equal(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildGreaterThanExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.GreaterThan(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildLessThanExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.LessThan(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildStartsWithExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var searchValue = ExtractStringValue(arg);
        if (searchValue == null) return null;

        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
        if (startsWithMethod == null) return null;

        var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
        var comparisonExpression = System.Linq.Expressions.Expression.Constant(StringComparison.OrdinalIgnoreCase);

        // Handle null strings
        var nullCheck = System.Linq.Expressions.Expression.NotEqual(
            property,
            System.Linq.Expressions.Expression.Constant(null, typeof(string))
        );

        var startsWithCall = System.Linq.Expressions.Expression.Call(
            property,
            startsWithMethod,
            searchValueExpression,
            comparisonExpression
        );

        return System.Linq.Expressions.Expression.AndAlso(nullCheck, startsWithCall);
    }

    private System.Linq.Expressions.Expression? BuildEndsWithExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var searchValue = ExtractStringValue(arg);
        if (searchValue == null) return null;

        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) });
        if (endsWithMethod == null) return null;

        var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
        var comparisonExpression = System.Linq.Expressions.Expression.Constant(StringComparison.OrdinalIgnoreCase);

        // Handle null strings
        var nullCheck = System.Linq.Expressions.Expression.NotEqual(
            property,
            System.Linq.Expressions.Expression.Constant(null, typeof(string))
        );

        var endsWithCall = System.Linq.Expressions.Expression.Call(
            property,
            endsWithMethod,
            searchValueExpression,
            comparisonExpression
        );

        return System.Linq.Expressions.Expression.AndAlso(nullCheck, endsWithCall);
    }

    private System.Linq.Expressions.Expression? BuildNotEqualsExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        // Special handling for string arrays - "not equals" means "does not contain"
        if (property.Type.IsArray && property.Type.GetElementType() == typeof(string))
        {
            var searchValue = ExtractStringValue(arg);
            if (searchValue == null) return null;

            // Use !Array.Contains to check if the array does not contain the value
            var containsMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(typeof(string));

            if (containsMethod == null) return null;

            var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);

            // !Enumerable.Contains(array, value)
            var containsCall = System.Linq.Expressions.Expression.Call(
                containsMethod,
                property,
                searchValueExpression
            );

            return System.Linq.Expressions.Expression.Not(containsCall);
        }

        var value = ExtractTypedValue(arg, property.Type);

        // Handle null comparison for "not equals"
        if (value == null || (value is string str && string.IsNullOrEmpty(str)))
        {
            // For null/empty string, return property != null
            return System.Linq.Expressions.Expression.NotEqual(
                property,
                System.Linq.Expressions.Expression.Constant(null, property.Type)
            );
        }

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.NotEqual(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildGreaterThanOrEqualExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.GreaterThanOrEqual(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildLessThanOrEqualExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value, property.Type);
        return System.Linq.Expressions.Expression.LessThanOrEqual(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildNotContainsExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        // Build the contains expression and negate it
        var containsExpression = BuildContainsExpression(property, args);
        return containsExpression != null
            ? System.Linq.Expressions.Expression.Not(containsExpression)
            : null;
    }

    private System.Linq.Expressions.Expression? BuildBlankExpression(System.Linq.Expressions.MemberExpression property)
    {
        try
        {
            logger?.LogDebug("Building blank expression for property {PropertyName} of type {PropertyType}", property.Member.Name, property.Type);

            var underlyingType = Nullable.GetUnderlyingType(property.Type);

            if (property.Type == typeof(string))
            {
                // For strings: property == null || property == ""
                var nullCheck = System.Linq.Expressions.Expression.Equal(
                    property,
                    System.Linq.Expressions.Expression.Constant(null, typeof(string))
                );

                var emptyCheck = System.Linq.Expressions.Expression.Equal(
                    property,
                    System.Linq.Expressions.Expression.Constant(string.Empty)
                );

                return System.Linq.Expressions.Expression.OrElse(nullCheck, emptyCheck);
            }
            else if (underlyingType != null)
            {
                // For nullable types: property == null
                return System.Linq.Expressions.Expression.Equal(
                    property,
                    System.Linq.Expressions.Expression.Constant(null, property.Type)
                );
            }
            else
            {
                // For non-nullable value types, "blank" doesn't make sense, but we'll return false
                return System.Linq.Expressions.Expression.Constant(false);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error building blank expression");
            throw;
        }
    }

    private System.Linq.Expressions.Expression? BuildNotBlankExpression(System.Linq.Expressions.MemberExpression property)
    {
        // Build the blank expression and negate it
        var blankExpression = BuildBlankExpression(property);
        return blankExpression != null
            ? System.Linq.Expressions.Expression.Not(blankExpression)
            : null;
    }

    private System.Linq.Expressions.Expression? BuildInRangeExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        try
        {
            logger?.LogDebug("Building in-range expression for property {PropertyName} of type {PropertyType}", property.Member.Name, property.Type);

            var argsList = args.ToList();
            if (argsList.Count < 2)
            {
                logger?.LogWarning("InRange requires 2 arguments, got {Count}", argsList.Count);
                return null;
            }

            var lowerBound = ExtractTypedValue(argsList[0], property.Type);
            var upperBound = ExtractTypedValue(argsList[1], property.Type);

            if (lowerBound == null || upperBound == null)
            {
                logger?.LogWarning("Failed to extract range bounds");
                return null;
            }

            // Create inclusive range: property >= lowerBound && property <= upperBound
            var lowerExpression = System.Linq.Expressions.Expression.Constant(lowerBound, property.Type);
            var upperExpression = System.Linq.Expressions.Expression.Constant(upperBound, property.Type);

            var greaterThanOrEqual = System.Linq.Expressions.Expression.GreaterThanOrEqual(property, lowerExpression);
            var lessThanOrEqual = System.Linq.Expressions.Expression.LessThanOrEqual(property, upperExpression);

            return System.Linq.Expressions.Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error building in-range expression");
            throw;
        }
    }

    private string? ExtractStringValue(Google.Protobuf.WellKnownTypes.Any arg)
    {
        try
        {
            logger?.LogDebug("Extracting string value from Any with TypeUrl: {TypeUrl}", arg.TypeUrl);

            // Check if it's a protobuf StringValue
            if (arg.Is(Google.Protobuf.WellKnownTypes.StringValue.Descriptor))
            {
                var stringValue = arg.Unpack<Google.Protobuf.WellKnownTypes.StringValue>();
                logger?.LogDebug("Unpacked StringValue: '{Result}'", stringValue.Value);
                return stringValue.Value;
            }

            // The frontend sends JSON-serialized strings, so we need to deserialize
            var jsonValue = arg.Value.ToStringUtf8();
            logger?.LogDebug("Raw value: '{JsonValue}'", jsonValue);

            var result = System.Text.Json.JsonSerializer.Deserialize<string>(jsonValue);
            logger?.LogDebug("Deserialized value: '{Result}'", result);

            return result;
        }
        catch (Exception ex)
        {
            logger?.LogDebug("JSON deserialization failed: {Message}", ex.Message);

            // Fallback: try to use the value directly
            var fallback = arg.Value.ToStringUtf8().Trim('"');
            logger?.LogDebug("Using fallback value: '{Fallback}'", fallback);

            return fallback;
        }
    }

    private object? ExtractTypedValue(Google.Protobuf.WellKnownTypes.Any arg, SystemType targetType)
    {
        try
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Try to unpack as protobuf well-known types first
            if (arg.Is(Google.Protobuf.WellKnownTypes.StringValue.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.StringValue>().Value;
                if (underlyingType == typeof(string)) return value;

                // Handle DateTime specially
                if (underlyingType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, out var dateTime))
                        return dateTime;
                }

                // Try to convert string to target type
                return Convert.ChangeType(value, underlyingType);
            }
            if (arg.Is(Google.Protobuf.WellKnownTypes.Int32Value.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.Int32Value>().Value;
                return Convert.ChangeType(value, underlyingType);
            }
            if (arg.Is(Google.Protobuf.WellKnownTypes.Int64Value.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.Int64Value>().Value;
                return Convert.ChangeType(value, underlyingType);
            }
            if (arg.Is(Google.Protobuf.WellKnownTypes.DoubleValue.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.DoubleValue>().Value;
                if (underlyingType == typeof(decimal))
                    return Convert.ToDecimal(value);  // Use Convert.ToDecimal for proper conversion
                if (underlyingType == typeof(float))
                    return Convert.ToSingle(value);
                return Convert.ChangeType(value, underlyingType);
            }
            if (arg.Is(Google.Protobuf.WellKnownTypes.FloatValue.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.FloatValue>().Value;
                return Convert.ChangeType(value, underlyingType);
            }
            if (arg.Is(Google.Protobuf.WellKnownTypes.BoolValue.Descriptor))
            {
                var value = arg.Unpack<Google.Protobuf.WellKnownTypes.BoolValue>().Value;
                return Convert.ChangeType(value, underlyingType);
            }

            // Fall back to JSON deserialization
            var jsonValue = arg.Value.ToStringUtf8();
            return underlyingType switch
            {
                SystemType t when t == typeof(string) => System.Text.Json.JsonSerializer.Deserialize<string>(jsonValue),
                SystemType t when t == typeof(int) => System.Text.Json.JsonSerializer.Deserialize<int>(jsonValue),
                SystemType t when t == typeof(long) => System.Text.Json.JsonSerializer.Deserialize<long>(jsonValue),
                SystemType t when t == typeof(double) => System.Text.Json.JsonSerializer.Deserialize<double>(jsonValue),
                SystemType t when t == typeof(float) => System.Text.Json.JsonSerializer.Deserialize<float>(jsonValue),
                SystemType t when t == typeof(bool) => System.Text.Json.JsonSerializer.Deserialize<bool>(jsonValue),
                SystemType t when t == typeof(DateTime) => System.Text.Json.JsonSerializer.Deserialize<DateTime>(jsonValue),
                SystemType t when t == typeof(decimal) => System.Text.Json.JsonSerializer.Deserialize<decimal>(jsonValue),
                _ => System.Text.Json.JsonSerializer.Deserialize<string>(jsonValue)
            };
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error extracting typed value for type {TargetType}", targetType);
            return null;
        }
    }

    private byte[] ConvertToArrowTable(List<object> data, IEnumerable<string> selectColumns, SystemType elementType, Func<object, object?>? idSelector = null)
    {
        logger?.LogDebug("Converting {DataCount} items to Arrow table", data.Count);

        var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Filter properties if selectColumns is specified
        if (selectColumns.Any())
        {
            properties = properties.Where(p => selectColumns.Contains(p.Name)).ToArray();
        }

        var fields = new List<ArrowField>();
        var arrays = new List<IArrowArray>();

        // Create schema and empty arrays even when there's no data
        foreach (var prop in properties)
        {
            var arrowType = QueryHelpers.GetArrowType(prop.PropertyType);
            fields.Add(new ArrowField(prop.Name, arrowType, nullable: true));

            // Create empty array if no data, otherwise create array with data
            if (!data.Any())
            {
                arrays.Add(QueryHelpers.CreateEmptyArrowArray(arrowType));
            }
            else
            {
                arrays.Add(QueryHelpers.CreateArrowArray(prop, data, arrowType));
            }
        }

        // Add _hiddenKey column if idSelector is provided
        const string HiddenKeyColumnName = "_hiddenKey";
        if (idSelector != null)
        {
            var hiddenKeyType = StringType.Default;
            fields.Add(new ArrowField(HiddenKeyColumnName, hiddenKeyType, nullable: true));

            if (!data.Any())
            {
                arrays.Add(QueryHelpers.CreateEmptyArrowArray(hiddenKeyType));
            }
            else
            {
                var hiddenKeyValues = data.Select(row =>
                {
                    var id = idSelector(row);
                    return id?.ToString() ?? null;
                }).ToList();
                arrays.Add(QueryHelpers.CreateStringArray(hiddenKeyValues.Cast<object?>().ToList()));
            }
        }

        var schema = new Schema(fields, null);
        var recordBatch = new RecordBatch(schema, arrays, data.Count);

        using var stream = new MemoryStream();
        using var writer = new ArrowStreamWriter(stream, schema);
        writer.WriteRecordBatch(recordBatch);
        writer.WriteEnd();

        var result = stream.ToArray();
        logger?.LogDebug("Created Arrow table with {ByteCount} bytes", result.Length);
        return result;
    }

    public ValuesResult ProcessValues(IQueryable queryable, DataTableValuesQuery query)
    {
        try
        {
            logger?.LogInformation("Processing values query for column: {Column}", query.Column);

            // Generate cache key if caching is enabled
            string? cacheKey = null;
            if (cache != null)
            {
                cacheKey = GenerateCacheKey("Values", queryable.ElementType.FullName, query);
                logger?.LogDebug("Generated cache key: {CacheKey}", cacheKey);

                // Try to get from cache
                var cachedData = cache.Get(cacheKey);
                if (cachedData != null)
                {
                    logger?.LogInformation("Cache hit for values query");
                    return DeserializeValuesResult(cachedData);
                }
                logger?.LogDebug("Cache miss for values query");
            }

            if (string.IsNullOrEmpty(query.Column))
            {
                throw new ArgumentException("Column name is required for values query");
            }

            var elementType = queryable.ElementType;
            var propertyInfo = elementType.GetProperty(query.Column);

            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"Column '{query.Column}' not found in type {elementType.Name}");
            }

            // Project to the specific column
            var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

            // Use Select to project to the column
            var selectMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Select" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(elementType, propertyInfo.PropertyType);

            if (selectMethod == null)
            {
                throw new InvalidOperationException("Could not find Select method");
            }

            var projectedQuery = (IQueryable)selectMethod.Invoke(null, new object[] { queryable, lambda })!;

            // Get distinct values
            var distinctMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Distinct" && m.GetParameters().Length == 1)?
                .MakeGenericMethod(propertyInfo.PropertyType);

            if (distinctMethod != null)
            {
                projectedQuery = (IQueryable)distinctMethod.Invoke(null, new object[] { projectedQuery })!;
            }

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(query.Search))
            {
                // Convert to string and filter
                var toStringMethod = propertyInfo.PropertyType.GetMethod("ToString", System.Type.EmptyTypes);
                if (toStringMethod != null || propertyInfo.PropertyType == typeof(string))
                {
                    var searchParameter = System.Linq.Expressions.Expression.Parameter(propertyInfo.PropertyType, "v");
                    System.Linq.Expressions.Expression searchExpression;

                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        // For string properties, use Contains directly
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
                        searchExpression = System.Linq.Expressions.Expression.Call(
                            searchParameter,
                            containsMethod!,
                            System.Linq.Expressions.Expression.Constant(query.Search),
                            System.Linq.Expressions.Expression.Constant(StringComparison.OrdinalIgnoreCase)
                        );
                    }
                    else
                    {
                        // For non-string properties, convert to string first
                        var toStringCall = System.Linq.Expressions.Expression.Call(searchParameter, toStringMethod!);
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
                        searchExpression = System.Linq.Expressions.Expression.Call(
                            toStringCall,
                            containsMethod!,
                            System.Linq.Expressions.Expression.Constant(query.Search),
                            System.Linq.Expressions.Expression.Constant(StringComparison.OrdinalIgnoreCase)
                        );
                    }

                    var searchLambda = System.Linq.Expressions.Expression.Lambda(searchExpression, searchParameter);

                    var whereMethod = typeof(Queryable).GetMethods()
                        .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2)?
                        .MakeGenericMethod(propertyInfo.PropertyType);

                    if (whereMethod != null)
                    {
                        projectedQuery = (IQueryable)whereMethod.Invoke(null, new object[] { projectedQuery, searchLambda })!;
                    }
                }
            }

            // Order by the column value
            var orderByMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(propertyInfo.PropertyType, propertyInfo.PropertyType);

            if (orderByMethod != null)
            {
                var orderParameter = System.Linq.Expressions.Expression.Parameter(propertyInfo.PropertyType, "v");
                var orderLambda = System.Linq.Expressions.Expression.Lambda(orderParameter, orderParameter);
                projectedQuery = (IQueryable)orderByMethod.Invoke(null, new object[] { projectedQuery, orderLambda })!;
            }

            // Execute query and convert to strings
            var rawValues = projectedQuery.Cast<object>()
                .Where(v => v != null)
                .ToList();

            // Handle string arrays - explode them into individual values
            List<string> values;
            int totalValues;
            if (propertyInfo.PropertyType.IsArray && propertyInfo.PropertyType.GetElementType() == typeof(string))
            {
                // Flatten arrays: each array element becomes a distinct value
                var allValues = rawValues
                    .SelectMany(v => v as string[] ?? System.Array.Empty<string>())
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                // Apply search filter on exploded values if provided
                if (!string.IsNullOrEmpty(query.Search))
                {
                    allValues = allValues
                        .Where(v => v.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                totalValues = allValues.Count;

                // Apply limit if specified
                if (query.Limit > 0)
                {
                    values = allValues.Take(query.Limit).ToList();
                }
                else
                {
                    values = allValues;
                }
            }
            else
            {
                totalValues = rawValues.Count;

                // Apply limit if specified
                var limitedValues = query.Limit > 0
                    ? rawValues.Take(query.Limit)
                    : rawValues;

                values = limitedValues
                    .Select(v => v.ToString()!)
                    .ToList();
            }

            logger?.LogInformation("Values query executed, got {ValueCount} values out of {TotalValues} total", values.Count, totalValues);

            var result = new ValuesResult
            {
                Values = values,
                TotalValues = totalValues
            };

            // Store in cache if enabled
            if (cache != null && cacheKey != null)
            {
                try
                {
                    var serialized = SerializeValuesResult(result);
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };
                    cache.Set(cacheKey, serialized, cacheOptions);
                    logger?.LogDebug("Stored values result in cache");
                }
                catch (Exception cacheEx)
                {
                    logger?.LogWarning(cacheEx, "Failed to cache values result");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error processing values query");
            throw;
        }
    }

    private string GenerateCacheKey(string prefix, string? typeName, object query)
    {
        using var sha256 = SHA256.Create();

        // Extract SourceId from the query object
        string sourceId = "";
        if (query is DataTableQuery dtQuery)
        {
            sourceId = dtQuery.SourceId ?? "";
        }
        else if (query is DataTableValuesQuery valQuery)
        {
            sourceId = valQuery.SourceId ?? "";
        }

        var jsonQuery = System.Text.Json.JsonSerializer.Serialize(query);
        var input = $"{prefix}:{sourceId}:{typeName}:{jsonQuery}";
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return $"DataTable:{prefix}:{sourceId}:{Convert.ToBase64String(hashBytes)}";
    }

    private byte[] SerializeQueryResult(QueryResult result)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(result.Offset);
        writer.Write(result.RowCount);
        writer.Write(result.TotalRows);
        writer.Write(result.ArrowData.Length);
        writer.Write(result.ArrowData);
        return stream.ToArray();
    }

    private QueryResult DeserializeQueryResult(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var result = new QueryResult
        {
            Offset = reader.ReadInt32(),
            RowCount = reader.ReadInt32(),
            TotalRows = reader.ReadInt32()
        };
        var arrowDataLength = reader.ReadInt32();
        result.ArrowData = reader.ReadBytes(arrowDataLength);
        return result;
    }

    private byte[] SerializeValuesResult(ValuesResult result)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        return Encoding.UTF8.GetBytes(json);
    }

    private ValuesResult DeserializeValuesResult(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return System.Text.Json.JsonSerializer.Deserialize<ValuesResult>(json) ?? new ValuesResult();
    }
}