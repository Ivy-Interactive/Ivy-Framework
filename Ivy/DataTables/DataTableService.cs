using System.Reactive.Disposables;
using Grpc.Core;
using IvyDataTables.Api.Protos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.DataTables;

public class DataTableService(IQueryableRegistry queryableRegistry) : TableService.TableServiceBase
{
    public override Task<TableResult> Query(TableQuery request, ServerCallContext context)
    {
        try
        {
            // Extract sourceId from the request
            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            var queryable = queryableRegistry.GetQueryable(request.SourceId);
            if (queryable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Queryable '{request.SourceId}' not found."));
            }

            var queryProcessor = new QueryProcessor();
            var arrowData = queryProcessor.ProcessQuery(queryable, request);

            var tableResult = new TableResult
            {
                ArrowIpcStream = Google.Protobuf.ByteString.CopyFrom(arrowData)
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
}