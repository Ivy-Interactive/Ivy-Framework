using Grpc.Core;
using Ivy.Filters;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

//todo: Check for JWT
//todo: We need the Widget

namespace Ivy.Views.DataTables;

public class DataTableService(
    IQueryableRegistry queryableRegistry,
    IDistributedCache? cache = null)
    : Protos.DataTable.DataTableService.DataTableServiceBase
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

            DataTableQuery queryToUse = request;

            // if (request.Filter != null && !string.IsNullOrWhiteSpace(request.Filter.InvalidQuery))
            // {
            //     var configuration = new ConfigurationBuilder()
            //         .AddUserSecrets<TableService>()
            //         .Build();

            //     var endpoint = configuration["OpenAi:Endpoint"] ?? throw new InvalidOperationException("OpenAi:Endpoint not found in user secrets");
            //     var apiKey = configuration["OpenAi:ApiKey"] ?? throw new InvalidOperationException("OpenAi:ApiKey not found in user secrets");

            //     // Create OpenAI client
            //     var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), new OpenAIClientOptions
            //     {
            //         Endpoint = new Uri(endpoint)
            //     });

            //     // Convert OpenAI ChatClient to IChatClient
            //     var openAIChatClient = openAiClient.GetChatClient("gpt-4o");
            //     var chatClient = openAIChatClient.AsIChatClient();

            //     var agent = new FilterParserAgent(chatClient, logger);
            //     var agentResult = await agent.Parse(request.Filter.InvalidQuery, queryable.ElementType.GetProperties().Select(p => new FieldMeta(p.Name, p.PropertyType)).ToArray());

            //     if (agentResult.HasErrors || agentResult.ProtoFilter == null)
            //     {
            //         var errorMessage = agentResult.Diagnostics.FirstOrDefault()?.Message ?? "Failed to parse filter query";
            //         throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid filter query: {errorMessage}"));
            //     }

            //     // Use the agent's parsed filter
            //     queryToUse = new DataTableQuery
            //     {
            //         SourceId = request.SourceId,
            //         ConnectionId = request.ConnectionId,
            //         Filter = (Filter)agentResult.ProtoFilter,
            //         Offset = request.Offset,
            //         Limit = request.Limit
            //     };
            //     queryToUse.Sort.AddRange(request.Sort);
            //     queryToUse.SelectColumns.AddRange(request.SelectColumns);
            //     queryToUse.Aggregations.AddRange(request.Aggregations);
            // }

            var queryProcessor = new QueryProcessor(logger: null, cache: cache);
            var queryResult = queryProcessor.ProcessQuery(queryable, queryToUse);

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