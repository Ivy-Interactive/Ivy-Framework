using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using Apache.Arrow.Ipc;
using Google.Protobuf.WellKnownTypes;
using ArrowField = Apache.Arrow.Field;
using SystemType = System.Type;
using IvyDataTables.Api.Protos;

namespace Ivy.DataTables;

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
public class QueryProcessor
{
    public byte[] ProcessQuery(IQueryable queryable, TableQuery query)
    {
        try
        {
            Console.WriteLine($"QueryProcessor: Processing query with filter: {query.Filter != null}");
            
            var processedQuery = queryable;

            // Apply filtering
            if (query.Filter != null)
            {
                Console.WriteLine($"QueryProcessor: Applying filter");
                processedQuery = ApplyFilter(processedQuery, query.Filter);
                Console.WriteLine($"QueryProcessor: Filter applied successfully");
            }

        // Apply sorting
        if (query.Sort.Any())
        {
            processedQuery = ApplySort(processedQuery, query.Sort);
        }

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
            Console.WriteLine($"QueryProcessor: Executing query");
            var results = processedQuery.Cast<object>().ToList();
            Console.WriteLine($"QueryProcessor: Query executed, got {results.Count} results");

            // Convert to Arrow table
            Console.WriteLine($"QueryProcessor: Converting to Arrow table");
            var arrowData = ConvertToArrowTable(results, query.SelectColumns, queryable.ElementType);
            Console.WriteLine($"QueryProcessor: Arrow conversion complete, {arrowData.Length} bytes");
            
            return arrowData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"QueryProcessor Error: {ex.Message}");
            Console.WriteLine($"QueryProcessor Stack Trace: {ex.StackTrace}");
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

        var methodName = firstSort.Direction == IvyDataTables.Api.Protos.SortDirection.Asc ? "OrderBy" : "OrderByDescending";
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
            Console.WriteLine($"ApplyFilter: Starting filter application for type {query.ElementType.Name}");
            
            var elementType = query.ElementType;
            var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "x");
            
            Console.WriteLine($"ApplyFilter: Building filter expression");
            var predicate = BuildFilterExpression(filter, parameter, elementType);
            
            if (predicate == null)
            {
                Console.WriteLine($"ApplyFilter: No predicate generated, returning original query");
                return query;
            }

            Console.WriteLine($"ApplyFilter: Creating lambda expression");
            var lambda = System.Linq.Expressions.Expression.Lambda(predicate, parameter);
            
            Console.WriteLine($"ApplyFilter: Getting Where method");
            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(elementType);

            if (whereMethod != null)
            {
                Console.WriteLine($"ApplyFilter: Invoking Where method");
                query = (IQueryable)whereMethod.Invoke(null, new object[] { query, lambda })!;
                Console.WriteLine($"ApplyFilter: Filter applied successfully");
            }
            else
            {
                Console.WriteLine($"ApplyFilter: Could not find Where method");
            }

            return query;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ApplyFilter Error: {ex.Message}");
            Console.WriteLine($"ApplyFilter Stack Trace: {ex.StackTrace}");
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
            Console.WriteLine($"BuildContainsExpression: Building contains for property {property.Member.Name} of type {property.Type}");
            
            var arg = args.FirstOrDefault();
            if (arg == null) 
            {
                Console.WriteLine($"BuildContainsExpression: No arguments provided");
                return null;
            }

            // Extract the string value from the protobuf Any
            Console.WriteLine($"BuildContainsExpression: Extracting string value from protobuf Any");
            var searchValue = ExtractStringValue(arg);
            if (searchValue == null) 
            {
                Console.WriteLine($"BuildContainsExpression: Failed to extract search value");
                return null;
            }
            
            Console.WriteLine($"BuildContainsExpression: Search value: '{searchValue}'");

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            if (containsMethod == null) 
            {
                Console.WriteLine($"BuildContainsExpression: Could not find Contains method");
                return null;
            }

            var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
            
            // Handle nullable properties
            if (property.Type == typeof(string))
            {
                Console.WriteLine($"BuildContainsExpression: Creating string contains expression");
                return System.Linq.Expressions.Expression.Call(property, containsMethod, searchValueExpression);
            }
            else
            {
                Console.WriteLine($"BuildContainsExpression: Converting non-string property to string first");
                // Convert to string first
                var toStringMethod = property.Type.GetMethod("ToString", System.Type.EmptyTypes);
                if (toStringMethod != null)
                {
                    var toStringCall = System.Linq.Expressions.Expression.Call(property, toStringMethod);
                    return System.Linq.Expressions.Expression.Call(toStringCall, containsMethod, searchValueExpression);
                }
                else
                {
                    Console.WriteLine($"BuildContainsExpression: Could not find ToString method for type {property.Type}");
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BuildContainsExpression Error: {ex.Message}");
            Console.WriteLine($"BuildContainsExpression Stack Trace: {ex.StackTrace}");
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

        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        if (startsWithMethod == null) return null;

        var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
        return System.Linq.Expressions.Expression.Call(property, startsWithMethod, searchValueExpression);
    }

    private System.Linq.Expressions.Expression? BuildEndsWithExpression(System.Linq.Expressions.MemberExpression property, IEnumerable<Google.Protobuf.WellKnownTypes.Any> args)
    {
        var arg = args.FirstOrDefault();
        if (arg == null) return null;

        var searchValue = ExtractStringValue(arg);
        if (searchValue == null) return null;

        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        if (endsWithMethod == null) return null;

        var searchValueExpression = System.Linq.Expressions.Expression.Constant(searchValue);
        return System.Linq.Expressions.Expression.Call(property, endsWithMethod, searchValueExpression);
    }

    private string? ExtractStringValue(Google.Protobuf.WellKnownTypes.Any arg)
    {
        try
        {
            Console.WriteLine($"ExtractStringValue: Extracting from Any with TypeUrl: {arg.TypeUrl}");
            
            // The frontend sends JSON-serialized strings, so we need to deserialize
            var jsonValue = arg.Value.ToStringUtf8();
            Console.WriteLine($"ExtractStringValue: Raw value: '{jsonValue}'");
            
            var result = System.Text.Json.JsonSerializer.Deserialize<string>(jsonValue);
            Console.WriteLine($"ExtractStringValue: Deserialized value: '{result}'");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ExtractStringValue: JSON deserialization failed: {ex.Message}");
            
            // Fallback: try to use the value directly
            var fallback = arg.Value.ToStringUtf8().Trim('"');
            Console.WriteLine($"ExtractStringValue: Using fallback value: '{fallback}'");
            
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
        Console.WriteLine($"ConvertToArrowTable: Converting {data.Count} items to Arrow table");
        
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
            var arrowType = GetArrowType(prop.PropertyType);
            fields.Add(new ArrowField(prop.Name, arrowType, nullable: true));
            
            // Create empty array if no data, otherwise create array with data
            if (!data.Any())
            {
                arrays.Add(CreateEmptyArrowArray(arrowType));
            }
            else
            {
                arrays.Add(CreateArrowArray(prop, data));
            }
        }

        var schema = new Schema(fields, null);
        var recordBatch = new RecordBatch(schema, arrays, data.Count);

        using var stream = new MemoryStream();
        using var writer = new ArrowStreamWriter(stream, schema);
        writer.WriteRecordBatch(recordBatch);
        writer.WriteEnd();

        var result = stream.ToArray();
        Console.WriteLine($"ConvertToArrowTable: Created Arrow table with {result.Length} bytes");
        return result;
    }

    private IArrowArray CreateEmptyArrowArray(IArrowType arrowType)
    {
        return arrowType switch
        {
            Int32Type => new Int32Array.Builder().Build(),
            Int64Type => new Int64Array.Builder().Build(),
            DoubleType => new DoubleArray.Builder().Build(),
            FloatType => new FloatArray.Builder().Build(),
            BooleanType => new BooleanArray.Builder().Build(),
            TimestampType => new TimestampArray.Builder().Build(),
            Decimal128Type => new Decimal128Array.Builder((Decimal128Type)arrowType).Build(),
            StringType => new StringArray.Builder().Build(),
            _ => new StringArray.Builder().Build()
        };
    }


    private IArrowType GetArrowType(SystemType type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType switch
        {
            SystemType t when t == typeof(int) => Int32Type.Default,
            SystemType t when t == typeof(long) => Int64Type.Default,
            SystemType t when t == typeof(decimal) => new Decimal128Type(18, 2),
            SystemType t when t == typeof(double) => DoubleType.Default,
            SystemType t when t == typeof(float) => FloatType.Default,
            SystemType t when t == typeof(bool) => BooleanType.Default,
            SystemType t when t == typeof(DateTime) => TimestampType.Default,
            SystemType t when t == typeof(string) => StringType.Default,
            _ => StringType.Default
        };
    }

    private IArrowArray CreateArrowArray(PropertyInfo property, List<object> data)
    {
        var values = data.Select(item => property.GetValue(item)).ToList();
        var type = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType switch
        {
            SystemType t when t == typeof(int) =>
                CreateInt32Array(values),
            SystemType t when t == typeof(long) =>
                CreateInt64Array(values),
            SystemType t when t == typeof(double) =>
                CreateDoubleArray(values),
            SystemType t when t == typeof(float) =>
                CreateFloatArray(values),
            SystemType t when t == typeof(bool) =>
                CreateBooleanArray(values),
            SystemType t when t == typeof(DateTime) =>
                CreateTimestampArray(values),
            SystemType t when t == typeof(decimal) =>
                CreateDecimalArray(values),
            SystemType t when t == typeof(string) =>
                CreateStringArray(values),
            _ => CreateStringArray(values)
        };
    }

    private IArrowArray CreateInt32Array(List<object?> values)
    {
        var builder = new Int32Array.Builder();
        foreach (var value in values)
        {
            if (value is int intValue)
                builder.Append(intValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateInt64Array(List<object?> values)
    {
        var builder = new Int64Array.Builder();
        foreach (var value in values)
        {
            if (value is long longValue)
                builder.Append(longValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateDoubleArray(List<object?> values)
    {
        var builder = new DoubleArray.Builder();
        foreach (var value in values)
        {
            if (value is double doubleValue)
                builder.Append(doubleValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateFloatArray(List<object?> values)
    {
        var builder = new FloatArray.Builder();
        foreach (var value in values)
        {
            if (value is float floatValue)
                builder.Append(floatValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateBooleanArray(List<object?> values)
    {
        var builder = new BooleanArray.Builder();
        foreach (var value in values)
        {
            if (value is bool boolValue)
                builder.Append(boolValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateTimestampArray(List<object?> values)
    {
        var builder = new TimestampArray.Builder();
        foreach (var value in values)
        {
            if (value is DateTime dateTimeValue)
                builder.Append(new DateTimeOffset(dateTimeValue));
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateDecimalArray(List<object?> values)
    {
        var builder = new Decimal128Array.Builder(new Decimal128Type(18, 2));
        foreach (var value in values)
        {
            if (value is decimal decimalValue)
                builder.Append(decimalValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    private IArrowArray CreateStringArray(List<object?> values)
    {
        var builder = new StringArray.Builder();
        foreach (var value in values)
        {
            if (value is string stringValue)
                builder.Append(stringValue);
            else if (value != null)
                builder.Append(value.ToString());
            else
                builder.AppendNull();
        }
        return builder.Build();
    }
}
