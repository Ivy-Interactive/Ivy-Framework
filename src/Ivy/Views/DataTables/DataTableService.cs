using Grpc.Core;
using Ivy.Filters;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ivy.Helpers;

namespace Ivy.Views.DataTables;

public class DataTableService(
    IQueryableRegistry queryableRegistry,
    AppSessionStore sessionStore,
    Server server,
    IDistributedCache? cache = null,
    IChatClient? chatClient = null,
    ILogger<DataTableService>? logger = null
    )
    : Protos.DataTable.DataTableService.DataTableServiceBase
{
    public override async Task<DataTableResult> Query(DataTableQuery request, ServerCallContext context)
    {
        try
        {
            await AuthHelper.ValidateAuthIfRequired(server, sessionStore, request.ConnectionId, context);

            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            var queryable = queryableRegistry.GetQueryable(request.SourceId);
            if (queryable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Queryable '{request.SourceId}' not found."));
            }

            var idSelector = queryableRegistry.GetIdSelector(request.SourceId);
            DataTableQuery queryToUse = request;

            var queryProcessor = new QueryProcessor(logger: null, cache: cache);
            var queryResult = queryProcessor.ProcessQuery(queryable, queryToUse, idSelector);

            var tableResult = new DataTableResult
            {
                ArrowIpcStream = Google.Protobuf.ByteString.CopyFrom(queryResult.ArrowData),
                Offset = queryResult.Offset,
                RowCount = queryResult.RowCount,
                TotalRows = queryResult.TotalRows
            };

            return tableResult;
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

    public override async Task<DataTableValuesResult> Values(DataTableValuesQuery request, ServerCallContext context)
    {
        try
        {
            await AuthHelper.ValidateAuthIfRequired(server, sessionStore, request.ConnectionId, context);

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

            return result;
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

    public override async Task<DataTableFilterParserResponse> ParseFilter(DataTableFilterParserRequest request, ServerCallContext context)
    {
        try
        {
            await AuthHelper.ValidateAuthIfRequired(server, sessionStore, request.ConnectionId, context);

            if (string.IsNullOrWhiteSpace(request.FilterExpression))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "FilterExpression is required in the request."));
            }

            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            var queryable = queryableRegistry.GetQueryable(request.SourceId);
            if (queryable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Queryable '{request.SourceId}' not found."));
            }

            if (chatClient == null)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "AI chat client is not configured. Cannot parse filter expressions."));
            }

            var fields = queryable.ElementType.GetProperties()
                .Select(p => new FieldMeta(p.Name, p.PropertyType))
                .ToArray();

            var agent = new FilterParserAgent(chatClient, logger);
            var agentResult = await agent.Parse(request.FilterExpression, fields);

            if (agentResult.HasErrors)
            {
                var errorMessage = agentResult.Diagnostics.FirstOrDefault()?.Message ?? "Failed to parse filter expression";
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid filter expression: {errorMessage}"));
            }

            return new DataTableFilterParserResponse
            {
                FilterExpression = agentResult.Filter
            };
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
