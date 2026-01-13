using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using System.Text;

namespace Ivy.Test.DataTables.TestHelpers;

/// <summary>
/// Helper class for working with Arrow data in tests
/// </summary>
public static class ArrowTestHelper
{
    /// <summary>
    /// Parses Arrow IPC stream bytes back into a RecordBatch
    /// </summary>
    public static RecordBatch ParseArrowData(byte[] arrowData)
    {
        using var stream = new MemoryStream(arrowData);
        using var reader = new ArrowStreamReader(stream);

        // Read the first (and typically only) batch
        var batch = reader.ReadNextRecordBatch();
        if (batch == null)
        {
            throw new InvalidOperationException("No record batch found in Arrow data");
        }

        return batch;
    }

    /// <summary>
    /// Gets column values as a list of objects
    /// </summary>
    public static List<object?> GetColumnValues(RecordBatch batch, string columnName)
    {
        var columnIndex = batch.Schema.GetFieldIndex(columnName);
        if (columnIndex == -1)
        {
            throw new ArgumentException($"Column '{columnName}' not found in schema");
        }

        return GetColumnValues(batch, columnIndex);
    }

    /// <summary>
    /// Gets column values by index as a list of objects
    /// </summary>
    public static List<object?> GetColumnValues(RecordBatch batch, int columnIndex)
    {
        var array = batch.Column(columnIndex);
        var values = new List<object?>();

        for (int i = 0; i < array.Length; i++)
        {
            if (array.IsNull(i))
            {
                values.Add(null);
            }
            else
            {
                values.Add(GetValue(array, i));
            }
        }

        return values;
    }

    private static object? GetValue(IArrowArray array, int index)
    {
        return array switch
        {
            Int8Array int8Array => int8Array.GetValue(index),
            Int16Array int16Array => int16Array.GetValue(index),
            Int32Array int32Array => int32Array.GetValue(index),
            Int64Array int64Array => int64Array.GetValue(index),
            UInt8Array uint8Array => uint8Array.GetValue(index),
            UInt16Array uint16Array => uint16Array.GetValue(index),
            UInt32Array uint32Array => uint32Array.GetValue(index),
            UInt64Array uint64Array => uint64Array.GetValue(index),
            FloatArray floatArray => floatArray.GetValue(index),
            DoubleArray doubleArray => doubleArray.GetValue(index),
            BooleanArray boolArray => boolArray.GetValue(index),
            StringArray stringArray => stringArray.GetString(index),
            TimestampArray timestampArray => timestampArray.GetTimestamp(index)?.DateTime,
            Decimal128Array decimalArray => GetDecimalValue(decimalArray, index),
            BinaryArray binaryArray => binaryArray.GetBytes(index).ToArray(),
            _ => throw new NotSupportedException($"Array type {array.GetType().Name} not supported")
        };
    }

    private static decimal? GetDecimalValue(Decimal128Array array, int index)
    {
        var sqlDecimal = array.GetValue(index);
        if (!sqlDecimal.HasValue) return null;

        // SqlDecimal can be explicitly cast to decimal
        return (decimal)sqlDecimal.Value;
    }

    /// <summary>
    /// Gets all rows as a list of dictionaries
    /// </summary>
    public static List<Dictionary<string, object?>> GetAllRows(RecordBatch batch)
    {
        var rows = new List<Dictionary<string, object?>>();

        for (int rowIndex = 0; rowIndex < batch.Length; rowIndex++)
        {
            var row = new Dictionary<string, object?>();

            for (int colIndex = 0; colIndex < batch.ColumnCount; colIndex++)
            {
                var fieldName = batch.Schema.GetFieldByIndex(colIndex).Name;
                var array = batch.Column(colIndex);

                if (array.IsNull(rowIndex))
                {
                    row[fieldName] = null;
                }
                else
                {
                    row[fieldName] = GetValue(array, rowIndex);
                }
            }

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Pretty prints the record batch for debugging
    /// </summary>
    public static string PrettyPrint(RecordBatch batch)
    {
        var sb = new StringBuilder();

        // Print schema
        sb.AppendLine("Schema:");
        foreach (var field in batch.Schema.FieldsList)
        {
            sb.AppendLine($"  {field.Name}: {field.DataType}");
        }

        // Print data
        sb.AppendLine($"\nData ({batch.Length} rows):");
        var rows = GetAllRows(batch);

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(", ", row.Select(kvp => $"{kvp.Key}={kvp.Value ?? "null"}")));
        }

        return sb.ToString();
    }
}