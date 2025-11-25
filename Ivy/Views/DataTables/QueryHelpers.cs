using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using SystemType = System.Type;

namespace Ivy.Views.DataTables;

public static class QueryHelpers
{
    public static IArrowArray CreateEmptyArrowArray(IArrowType arrowType)
    {
        return arrowType switch
        {
            Int8Type => new Int8Array.Builder().Build(),
            Int16Type => new Int16Array.Builder().Build(),
            Int32Type => new Int32Array.Builder().Build(),
            Int64Type => new Int64Array.Builder().Build(),
            UInt8Type => new UInt8Array.Builder().Build(),
            UInt16Type => new UInt16Array.Builder().Build(),
            UInt32Type => new UInt32Array.Builder().Build(),
            UInt64Type => new UInt64Array.Builder().Build(),
            DoubleType => new DoubleArray.Builder().Build(),
            FloatType => new FloatArray.Builder().Build(),
            BooleanType => new BooleanArray.Builder().Build(),
            TimestampType => new TimestampArray.Builder().Build(),
            Decimal128Type => new Decimal128Array.Builder((Decimal128Type)arrowType).Build(),
            BinaryType => new BinaryArray.Builder().Build(),
            StringType => new StringArray.Builder().Build(),
            _ => new StringArray.Builder().Build()
        };
    }

    public static IArrowType GetArrowType(SystemType type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType switch
        {
            SystemType t when t == typeof(sbyte) => Int8Type.Default,
            SystemType t when t == typeof(byte) => UInt8Type.Default,
            SystemType t when t == typeof(short) => Int16Type.Default,
            SystemType t when t == typeof(ushort) => UInt16Type.Default,
            SystemType t when t == typeof(int) => Int32Type.Default,
            SystemType t when t == typeof(uint) => UInt32Type.Default,
            SystemType t when t == typeof(long) => Int64Type.Default,
            SystemType t when t == typeof(ulong) => UInt64Type.Default,
            SystemType t when t == typeof(decimal) => new Decimal128Type(38, 28),
            SystemType t when t == typeof(double) => DoubleType.Default,
            SystemType t when t == typeof(float) => FloatType.Default,
            SystemType t when t == typeof(bool) => BooleanType.Default,
            SystemType t when t == typeof(DateTime) => TimestampType.Default,
            SystemType t when t == typeof(DateTimeOffset) => TimestampType.Default,
            SystemType t when t == typeof(TimeSpan) => Int64Type.Default, // Store as microseconds
            SystemType t when t == typeof(char) => StringType.Default,
            SystemType t when t == typeof(Guid) => StringType.Default,
            SystemType t when t == typeof(byte[]) => BinaryType.Default,
            SystemType t when t == typeof(string) => StringType.Default,
            SystemType t when t.IsArray && t.GetElementType() == typeof(string) => StringType.Default, // Serialize string arrays as JSON
            _ => StringType.Default
        };
    }

    public static IArrowArray CreateArrowArray(PropertyInfo property, List<object> data, IArrowType? arrowType = null)
    {
        var values = data.Select(property.GetValue).ToList();
        var type = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Handle string arrays - serialize as JSON strings
        if (underlyingType.IsArray && underlyingType.GetElementType() == typeof(string))
        {
            return CreateStringArrayAsJson(values);
        }

        return underlyingType switch
        {
            SystemType t when t == typeof(sbyte) =>
                CreateInt8Array(values),
            SystemType t when t == typeof(byte) =>
                CreateUInt8Array(values),
            SystemType t when t == typeof(short) =>
                CreateInt16Array(values),
            SystemType t when t == typeof(ushort) =>
                CreateUInt16Array(values),
            SystemType t when t == typeof(int) =>
                CreateInt32Array(values),
            SystemType t when t == typeof(uint) =>
                CreateUInt32Array(values),
            SystemType t when t == typeof(long) =>
                CreateInt64Array(values),
            SystemType t when t == typeof(ulong) =>
                CreateUInt64Array(values),
            SystemType t when t == typeof(double) =>
                CreateDoubleArray(values),
            SystemType t when t == typeof(float) =>
                CreateFloatArray(values),
            SystemType t when t == typeof(bool) =>
                CreateBooleanArray(values),
            SystemType t when t == typeof(DateTime) =>
                CreateTimestampArray(values),
            SystemType t when t == typeof(DateTimeOffset) =>
                CreateDateTimeOffsetArray(values),
            SystemType t when t == typeof(TimeSpan) =>
                CreateTimeSpanArray(values),
            SystemType t when t == typeof(decimal) =>
                CreateDecimalArray(values, arrowType as Decimal128Type),
            SystemType t when t == typeof(char) =>
                CreateCharArray(values),
            SystemType t when t == typeof(Guid) =>
                CreateGuidArray(values),
            SystemType t when t == typeof(byte[]) =>
                CreateBinaryArray(values),
            SystemType t when t == typeof(string) =>
                CreateStringArray(values),
            _ => CreateStringArray(values)
        };
    }

    public static IArrowArray CreateInt32Array(List<object?> values)
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

    public static IArrowArray CreateInt64Array(List<object?> values)
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

    public static IArrowArray CreateDoubleArray(List<object?> values)
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

    public static IArrowArray CreateFloatArray(List<object?> values)
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

    public static IArrowArray CreateBooleanArray(List<object?> values)
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

    public static IArrowArray CreateTimestampArray(List<object?> values)
    {
        var builder = new TimestampArray.Builder();
        foreach (var value in values)
        {
            if (value is DateTime dateTimeValue)
            {
                // Handle DateTime properly - check if it's within valid range
                if (dateTimeValue >= DateTimeOffset.MinValue.DateTime &&
                    dateTimeValue <= DateTimeOffset.MaxValue.DateTime)
                {
                    // If DateTime.Kind is Unspecified, treat as local
                    if (dateTimeValue.Kind == DateTimeKind.Unspecified)
                        dateTimeValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Local);

                    builder.Append(new DateTimeOffset(dateTimeValue));
                }
                else
                {
                    builder.AppendNull();
                }
            }
            else
            {
                builder.AppendNull();
            }
        }
        return builder.Build();
    }

    public static IArrowArray CreateDecimalArray(List<object?> values, Decimal128Type? decimalType = null)
    {
        // If no type is provided, determine the scale dynamically
        if (decimalType == null)
        {
            int maxScale = 28; // Default scale matching GetArrowType
            foreach (var value in values)
            {
                if (value is decimal decimalValue)
                {
                    var scale = BitConverter.GetBytes(decimal.GetBits(decimalValue)[3])[2];
                    if (scale > maxScale)
                        maxScale = scale;
                }
            }
            decimalType = new Decimal128Type(38, maxScale);
        }

        var builder = new Decimal128Array.Builder(decimalType);
        foreach (var value in values)
        {
            if (value is decimal decimalValue)
                builder.Append(decimalValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateStringArray(List<object?> values)
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

    public static IArrowArray CreateStringArrayAsJson(List<object?> values)
    {
        var builder = new StringArray.Builder();
        foreach (var value in values)
        {
            if (value is string[] stringArray)
            {
                // Serialize array as JSON
                var json = System.Text.Json.JsonSerializer.Serialize(stringArray);
                builder.Append(json);
            }
            else if (value == null)
            {
                builder.AppendNull();
            }
            else
            {
                // Fallback: serialize as single-item array
                var json = System.Text.Json.JsonSerializer.Serialize(new[] { value.ToString() });
                builder.Append(json);
            }
        }
        return builder.Build();
    }

    public static IArrowArray CreateInt8Array(List<object?> values)
    {
        var builder = new Int8Array.Builder();
        foreach (var value in values)
        {
            if (value is sbyte sbyteValue)
                builder.Append(sbyteValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateUInt8Array(List<object?> values)
    {
        var builder = new UInt8Array.Builder();
        foreach (var value in values)
        {
            if (value is byte byteValue)
                builder.Append(byteValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateInt16Array(List<object?> values)
    {
        var builder = new Int16Array.Builder();
        foreach (var value in values)
        {
            if (value is short shortValue)
                builder.Append(shortValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateUInt16Array(List<object?> values)
    {
        var builder = new UInt16Array.Builder();
        foreach (var value in values)
        {
            if (value is ushort ushortValue)
                builder.Append(ushortValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateUInt32Array(List<object?> values)
    {
        var builder = new UInt32Array.Builder();
        foreach (var value in values)
        {
            if (value is uint uintValue)
                builder.Append(uintValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateUInt64Array(List<object?> values)
    {
        var builder = new UInt64Array.Builder();
        foreach (var value in values)
        {
            if (value is ulong ulongValue)
                builder.Append(ulongValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateDateTimeOffsetArray(List<object?> values)
    {
        var builder = new TimestampArray.Builder();
        foreach (var value in values)
        {
            if (value is DateTimeOffset dateTimeOffsetValue)
                builder.Append(dateTimeOffsetValue);
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateTimeSpanArray(List<object?> values)
    {
        // Store TimeSpan as Int64 (microseconds)
        var builder = new Int64Array.Builder();
        foreach (var value in values)
        {
            if (value is TimeSpan timeSpanValue)
                builder.Append(timeSpanValue.Ticks / 10); // Convert ticks to microseconds
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateCharArray(List<object?> values)
    {
        var builder = new StringArray.Builder();
        foreach (var value in values)
        {
            if (value is char charValue)
                builder.Append(charValue.ToString());
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateGuidArray(List<object?> values)
    {
        var builder = new StringArray.Builder();
        foreach (var value in values)
        {
            if (value is Guid guidValue)
                builder.Append(guidValue.ToString());
            else
                builder.AppendNull();
        }
        return builder.Build();
    }

    public static IArrowArray CreateBinaryArray(List<object?> values)
    {
        var builder = new BinaryArray.Builder();
        foreach (var value in values)
        {
            if (value is byte[] byteArrayValue)
                builder.Append(byteArrayValue.AsSpan());
            else
                builder.AppendNull();
        }
        return builder.Build();
    }
}