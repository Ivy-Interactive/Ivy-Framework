using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery;

internal static class SqlExecutor
{
    public static async Task<SqlExecutionResult> ExecuteAsync(DbContext context, string sql, CancellationToken ct)
    {
        var validationError = SqlValidator.Validate(sql);
        if (validationError != null)
            return new SqlExecutionResult
            {
                Columns = [],
                Rows = [],
                TotalRowCount = 0,
                Error = validationError
            };

        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 30;
            command.Transaction = transaction;

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, ct);

            var columns = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
                columns[i] = reader.GetName(i);

            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(ct) && rows.Count < 1000)
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[columns[i]] = value == DBNull.Value ? null : value;
                }
                rows.Add(row);
            }

            var totalRowCount = rows.Count;
            if (await reader.ReadAsync(ct))
                totalRowCount = -1; // indicates more rows exist

            await transaction.RollbackAsync(ct);

            return new SqlExecutionResult
            {
                Columns = columns,
                Rows = rows,
                TotalRowCount = totalRowCount
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new SqlExecutionResult
            {
                Columns = [],
                Rows = [],
                TotalRowCount = 0,
                Error = ex.Message
            };
        }
    }
}

internal record SqlExecutionResult
{
    public required string[] Columns { get; init; }
    public required List<Dictionary<string, object?>> Rows { get; init; }
    public required int TotalRowCount { get; init; }
    public string? Error { get; init; }
}
