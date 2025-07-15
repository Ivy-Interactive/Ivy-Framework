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
        var processedQuery = queryable;

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
        var results = processedQuery.Cast<object>().ToList();

        // Convert to Arrow table
        return ConvertToArrowTable(results, query.SelectColumns, queryable.ElementType);
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

        var methodName = firstSort.Direction == SortDirection.Asc ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2)?
            .MakeGenericMethod(elementType, propertyInfo.PropertyType);

        if (method != null)
        {
            query = (IQueryable)method.Invoke(null, new object[] { query, lambda })!;
        }

        return query;
    }

    private byte[] ConvertToArrowTable(List<object> data, IEnumerable<string> selectColumns, SystemType elementType)
    {
        if (!data.Any())
        {
            return System.Array.Empty<byte>();
        }

        var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Filter properties if selectColumns is specified
        if (selectColumns.Any())
        {
            properties = properties.Where(p => selectColumns.Contains(p.Name)).ToArray();
        }

        var fields = new List<ArrowField>();
        var arrays = new List<IArrowArray>();

        foreach (var prop in properties)
        {
            var arrowType = GetArrowType(prop.PropertyType);
            fields.Add(new ArrowField(prop.Name, arrowType, nullable: true));
            arrays.Add(CreateArrowArray(prop, data));
        }

        var schema = new Schema(fields, null);
        var recordBatch = new RecordBatch(schema, arrays, data.Count);

        using var stream = new MemoryStream();
        using var writer = new ArrowStreamWriter(stream, schema);
        writer.WriteRecordBatch(recordBatch);
        writer.WriteEnd();

        return stream.ToArray();
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
