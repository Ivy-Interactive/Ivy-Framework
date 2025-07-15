using System.Reactive.Disposables;
using Grpc.Core;
using IvyDataTables.Api.Protos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.DataTables;

public class DataTableService(string connectionId, ServerArgs serverArgs) : TableService.TableServiceBase, IDataTableService
{
    private readonly Dictionary<Guid, IQueryable> _queryables = new();
    
    public override Task<TableResult> Query(TableQuery request, ServerCallContext context)
    {
        try
        {
            // Extract sourceId from the request
            if (string.IsNullOrEmpty(request.SourceId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required in the request."));
            }

            if (!_queryables.TryGetValue(Guid.Parse(request.SourceId), out var queryable))
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
    
    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable)
    {
        Console.WriteLine($"Adding queryable with connectionId: {connectionId}");
        var sourceId = Guid.NewGuid();
        _queryables[sourceId] = queryable;

        var cleanup = Disposable.Create(() =>
        {
            _queryables.Remove(sourceId);
        });

        var connection = new DataTableConnection(serverArgs.Port, "/datatable.TableService/Query/", connectionId, sourceId.ToString());
        
        return (cleanup, connection);
    }
}

public interface IDataTableService
{
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable);
    Task<TableResult> Query(TableQuery request, ServerCallContext context);
}