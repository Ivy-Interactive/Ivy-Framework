using Grpc.Core;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

//todo: Check for JWT
//todo: We need the Widget

namespace Ivy.Views.DataTables;

public class TableService(
    IQueryableRegistry queryableRegistry,
    ILogger<TableService>? logger = null,
    IDistributedCache? cache = null)
    : DataTableService.DataTableServiceBase
{
    public override Task<DataTableResult> Query(DataTableQuery request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            var queryable = queryableRegistry.GetQueryable(request.SourceId);
            if (queryable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Queryable '{request.SourceId}' not found."));
            }

            // TODO: If filter has invalid_query set, use FilterParserAgent to convert it
            // This requires:
            // 1. Adding Ivy.Filters project reference
            // 2. Injecting IChatClient into the constructor
            // 3. Calling ProcessInvalidQuery(request, queryable) here
            if (request.Filter != null && !string.IsNullOrWhiteSpace(request.Filter.InvalidQuery))
            {
                logger?.LogWarning("Invalid query detected but agent processing not implemented: {Query}",
                    request.Filter.InvalidQuery);
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Invalid filter query: {request.Filter.InvalidQuery}. Agent conversion not yet configured."));
            }

            var queryProcessor = new QueryProcessor(logger: null, cache: cache);
            var queryResult = queryProcessor.ProcessQuery(queryable, request);

            var tableResult = new DataTableResult
            {
                ArrowIpcStream = Google.Protobuf.ByteString.CopyFrom(queryResult.ArrowData),
                Offset = queryResult.Offset,
                RowCount = queryResult.RowCount,
                TotalRows = queryResult.TotalRows
            };

            return Task.FromResult(tableResult);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {ex.Message}"));
        }
    }

    // TODO: Uncomment and implement when Ivy.Filters is available
    // private async Task ProcessInvalidQuery(DataTableQuery request, IQueryable queryable)
    // {
    //     var invalidQuery = request.Filter!.InvalidQuery;
    //     logger?.LogInformation("Processing invalid query with agent: {Query}", invalidQuery);
    //
    //     if (chatClient == null)
    //     {
    //         throw new RpcException(new Status(StatusCode.InvalidArgument,
    //             "Invalid filter query and no AI chat client configured for conversion."));
    //     }
    //
    //     // Extract fields from queryable
    //     var fields = ExtractFieldsFromQueryable(queryable);
    //
    //     // Use agent to convert the invalid query
    //     var agent = new FilterParserAgent(chatClient, logger);
    //     var agentResult = await agent.Parse(invalidQuery, fields);
    //
    //     if (agentResult.HasErrors || agentResult.Model == null)
    //     {
    //         var errorMessage = string.Join(", ", agentResult.Diagnostics.Select(d => d.Message));
    //         logger?.LogError("Agent failed to convert query: {Errors}", errorMessage);
    //         throw new RpcException(new Status(StatusCode.InvalidArgument,
    //             $"Could not parse filter query: {errorMessage}"));
    //     }
    //
    //     // Replace the filter with the agent-converted one
    //     request.Filter = agentResult.Model;
    //     logger?.LogInformation("Successfully converted invalid query using agent");
    // }

    public override Task<DataTableValuesResult> Values(DataTableValuesQuery request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            var queryable = queryableRegistry.GetQueryable(request.SourceId);
            if (queryable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Queryable '{request.SourceId}' not found."));
            }

            var queryProcessor = new QueryProcessor(logger: null, cache: cache);
            var valuesResult = queryProcessor.ProcessValues(queryable, request);

            var result = new DataTableValuesResult
            {
                TotalValues = valuesResult.TotalValues
            };
            result.Values.AddRange(valuesResult.Values);

            return Task.FromResult(result);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {ex.Message}"));
        }
    }
}