using Grpc.Core;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

//todo: Check for JWT
//todo: We need the Widget 

namespace Ivy.Views.DataTables;

public class TableService(IQueryableRegistry queryableRegistry, ILogger<TableService>? logger = null, IDistributedCache? cache = null)
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