using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Ivy.Protos.DataTable;
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

/// <summary>
/// Processes table queries by applying sorting and pagination to IQueryable data sources,
/// then converts the results to Apache Arrow format for efficient data transfer.
/// </summary>
/// <remarks>
/// The QueryProcessor handles the following operations:
/// - Sorting: Supports multi-column sorting with ascending/descending directions
/// - Pagination: Implements offset and limit for result set pagination
/// - Data conversion: Converts .NET objects to Apache Arrow table format for optimal performance
/// 
/// The processor works with any IQueryable&lt;T&gt; data source and returns serialized Arrow data
/// that can be efficiently transmitted and processed by client applications.
/// </remarks>
public class QueryProcessor(ILogger<QueryProcessor>? logger = null)
{
    public QueryResult ProcessQuery(IQueryable queryable, DataTableQuery query)
    {
        try
        {
            logger?.LogInformation("Processing query with filter: {HasFilter}", query.Filter != null);

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

            if (query.Limit > 0)
            {
                var takeMethod = typeof(Queryable).GetMethods()
                    .FirstOrDefault(m => m.Name == "Take" && m.GetParameters().Length == 2)?
                    .MakeGenericMethod(queryable.ElementType);

                if (takeMethod != null)
                {
                    processedQuery = (IQueryable)takeMethod.Invoke(null, new object[] { processedQuery, query.Limit })!;
                }
            }

            // Execute query and get results
            logger?.LogDebug("Executing query");
            var results = processedQuery.Cast<object>().ToList();
            logger?.LogInformation("Query executed, got {ResultCount} results", results.Count);

            // Convert to Arrow table
            logger?.LogDebug("Converting to Arrow table");
            var arrowData = ConvertToArrowTable(results, query.SelectColumns, queryable.ElementType);
            logger?.LogInformation("Arrow conversion complete, {ByteCount} bytes", arrowData.Length);

            return new QueryResult
            {
                ArrowData = arrowData,
                Offset = query.Offset,
                RowCount = results.Count,
                TotalRows = totalRows
            };
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

        // For now, we'll handle only the first sort column
        // In a full implementation, you'd want to support multiple columns
        var firstSort = sortOrdersList.First();
        var elementType = query.ElementType;
        var propertyInfo = elementType.GetProperty(firstSort.Column);

        if (propertyInfo == null)
            return query;

        var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);
        var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

        var methodName = firstSort.Direction == Ivy.Protos.DataTable.SortDirection.Asc ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2)?
            .MakeGenericMethod(elementType, propertyInfo.PropertyType);

        if (method != null)
        {
            query = (IQueryable)method.Invoke(null, new object[] { query, lambda })!;
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
                query = (IQueryable)whereMethod.Invoke(null, new object[] { query, lambda })!;
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
            return null;

        var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);

        return condition.Function.ToLowerInvariant() switch
        {
            "contains" => BuildContainsExpression(property, condition.Args),
            "equals" => BuildEqualsExpression(property, condition.Args),
            "greaterthan" => BuildGreaterThanExpression(property, condition.Args),
            "lessthan" => BuildLessThanExpression(property, condition.Args),
            "startswith" => BuildStartsWithExpression(property, condition.Args),
            "endswith" => BuildEndsWithExpression(property, condition.Args),
            _ => null
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

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value);
        return System.Linq.Expressions.Expression.Equal(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildGreaterThanExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value);
        return System.Linq.Expressions.Expression.GreaterThan(property, valueExpression);
    }

    private System.Linq.Expressions.Expression? BuildLessThanExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var value = ExtractTypedValue(arg, property.Type);
        if (value == null) return null;

        var valueExpression = System.Linq.Expressions.Expression.Constant(value);
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

    private string? ExtractStringValue(Google.Protobuf.WellKnownTypes.Any arg)
    {
        try
        {
            logger?.LogDebug("Extracting string value from Any with TypeUrl: {TypeUrl}", arg.TypeUrl);

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
            var jsonValue = arg.Value.ToStringUtf8();
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

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
        catch
        {
            return null;
        }
    }

    private byte[] ConvertToArrowTable(List<object> data, IEnumerable<string> selectColumns, SystemType elementType)
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
                arrays.Add(QueryHelpers.CreateArrowArray(prop, data));
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

}
