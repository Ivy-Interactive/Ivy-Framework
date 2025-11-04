using Grpc.Core;
using Ivy.Auth;
using Ivy.Filters;
using Ivy.Protos.DataTable;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text.Json;

namespace Ivy.Views.DataTables;

public class DataTableService(
    IQueryableRegistry queryableRegistry,
    Server server,
    IServiceProvider serviceProvider,
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
            await ValidateAuthIfRequired(context);

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

            var queryProcessor = new QueryProcessor(logger: null, cache: cache);
            var queryResult = queryProcessor.ProcessQuery(queryable, queryToUse);

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
            await ValidateAuthIfRequired(context);

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
            await ValidateAuthIfRequired(context);

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

    private AuthToken? GetAuthToken(ServerCallContext context)
    {
        var cookies = context.RequestHeaders.GetValue("cookie") ?? string.Empty;
        if (string.IsNullOrEmpty(cookies))
        {
            return null;
        }

        var cookieHeader = CookieHeaderValue.ParseList([cookies]).ToList();
        var rawAuthTokenValue = cookieHeader
            .FirstOrDefault(c => c.Name.Equals("auth_token", StringComparison.OrdinalIgnoreCase))?.Value.Value;

        if (string.IsNullOrEmpty(rawAuthTokenValue))
        {
            return null;
        }

        var authTokenValue = WebUtility.UrlDecode(rawAuthTokenValue);

        try
        {
            var token = JsonSerializer.Deserialize<AuthToken>(authTokenValue);
            if (token == null)
            {
                return null;
            }

            // Check if refresh token is in a separate cookie
            if (token.RefreshToken == null)
            {
                var refreshTokenValue = cookieHeader
                    .FirstOrDefault(c => c.Name.Equals("auth_ext_refresh_token", StringComparison.OrdinalIgnoreCase))?.Value.Value;
                return token with { RefreshToken = refreshTokenValue };
            }

            return token;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to deserialize AuthToken from cookies.");
            return null;
        }
    }

    private async Task ValidateAuthIfRequired(ServerCallContext context)
    {
        // Check if auth is required
        if (server.AuthProviderType == null)
        {
            return;
        }

        var authToken = GetAuthToken(context);
        if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication required."));
        }

        // Get auth provider and validate token
        var authProvider = serviceProvider.GetService<IAuthProvider>()
            ?? throw new RpcException(new Status(StatusCode.Internal, "Auth provider not configured."));

        try
        {
            var isValid = await authProvider.ValidateAccessTokenAsync(authToken.AccessToken, context.CancellationToken);
            if (!isValid)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired authentication token."));
            }
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error validating auth token.");
            throw new RpcException(new Status(StatusCode.Internal, "Error validating auth token."));
        }
    }
}
