using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Ivy.Plugins.Messaging;

namespace Ivy.Plugin.Slack;

public sealed class SlackMessagingChannel : IMessagingChannel, IDisposable
{
    private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
    private const string DeleteMessageUrl = "https://slack.com/api/chat.delete";
    private const string GetUploadUrlUrl = "https://slack.com/api/files.getUploadURLExternal";
    private const string CompleteUploadUrl = "https://slack.com/api/files.completeUploadExternal";
    private const string ConversationsListUrl = "https://slack.com/api/conversations.list";
    private const string FilesInfoUrl = "https://slack.com/api/files.info";

    private readonly HttpClient _http;
    private readonly int _maxRetries;

    public SlackMessagingChannel(SlackConfig config)
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config.BotToken);
        DefaultChannel = config.DefaultChannel;
        _maxRetries = config.MaxRetries;
    }

    public string Platform => "slack";
    public string? DefaultChannel { get; }

    public async Task<MessageResult> SendMessageAsync(string channel, Message message, CancellationToken ct = default)
    {
        if (message.Attachments is { Count: > 0 })
            return await SendWithAttachments(channel, message, ct);

        return await SendTextMessage(channel, message, ct);
    }

    private async Task<MessageResult> SendTextMessage(string channel, Message message, CancellationToken ct)
    {
        var (blocks, fallbackText) = SlackRenderer.Render(message.Content);

        var payload = new JsonObject
        {
            ["channel"] = channel,
            ["text"] = fallbackText,
            ["blocks"] = blocks,
        };

        if (message.ThreadId is not null)
            payload["thread_ts"] = message.ThreadId;

        var content = new StringContent(
            payload.ToJsonString(),
            Encoding.UTF8,
            "application/json");

        var response = await _http.PostAsync(PostMessageUrl, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Slack API returned {response.StatusCode}: {body}");

        var result = JsonNode.Parse(body);
        if (result?["ok"]?.GetValue<bool>() != true)
        {
            var error = result?["error"]?.GetValue<string>() ?? "unknown error";
            throw new InvalidOperationException($"Slack API error: {error}");
        }

        var ts = result["ts"]?.GetValue<string>() ?? "";
        var responseChannel = result["channel"]?.GetValue<string>() ?? channel;

        return new MessageResult
        {
            MessageId = ts,
            ThreadId = ts,
            Channel = responseChannel,
            Timestamp = ParseSlackTimestamp(ts),
        };
    }

    private async Task<MessageResult> SendWithAttachments(string channel, Message message, CancellationToken ct)
    {
        var channelId = await ResolveChannelId(channel, ct);

        // Step 1: Upload all files and collect their IDs
        var fileEntries = new JsonArray();
        foreach (var attachment in message.Attachments!)
        {
            using var stream = new MemoryStream(attachment.Content);
            var fileId = await UploadFileContent(stream, attachment.FileName, ct);
            fileEntries.Add(new JsonObject
            {
                ["id"] = fileId,
                ["title"] = attachment.Title ?? attachment.FileName,
            });
        }

        // Step 2: Complete upload with blocks for a single unified message
        var (blocks, _) = SlackRenderer.Render(message.Content);

        var completePayload = new JsonObject
        {
            ["files"] = fileEntries,
            ["channel_id"] = channelId,
            ["blocks"] = blocks,
        };

        if (message.ThreadId is not null)
            completePayload["thread_ts"] = message.ThreadId;

        var completeContent = new StringContent(
            completePayload.ToJsonString(), Encoding.UTF8, "application/json");
        var completeResponse = await _http.PostAsync(CompleteUploadUrl, completeContent, ct);
        var completeBody = await completeResponse.Content.ReadAsStringAsync(ct);
        var completeResult = JsonNode.Parse(completeBody);

        if (completeResult?["ok"]?.GetValue<bool>() != true)
        {
            var error = completeResult?["error"]?.GetValue<string>() ?? "unknown error";
            throw new InvalidOperationException($"Slack files.completeUploadExternal error: {error}");
        }

        var firstFileId = fileEntries[0]!["id"]!.GetValue<string>();
        var ts = await GetFileMessageTs(firstFileId, channelId, ct);

        // For threaded file uploads, shares may not be populated;
        // fall back to the parent thread ts so reply chains still work
        if (string.IsNullOrEmpty(ts))
            ts = message.ThreadId ?? "";

        return new MessageResult
        {
            MessageId = ts,
            ThreadId = message.ThreadId ?? ts,
            Channel = channelId,
            Timestamp = ts.Length > 0 ? ParseSlackTimestamp(ts) : DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Uploads file content and returns the file ID (steps 1-2 of the upload flow).
    /// </summary>
    private async Task<string> UploadFileContent(Stream content, string fileName, CancellationToken ct)
    {
        var length = content.CanSeek ? content.Length : throw new ArgumentException("Stream must be seekable.");
        var getUrlParams = $"?filename={Uri.EscapeDataString(fileName)}&length={length}";
        var getUrlResponse = await _http.GetAsync(GetUploadUrlUrl + getUrlParams, ct);
        var getUrlBody = await getUrlResponse.Content.ReadAsStringAsync(ct);
        var getUrlResult = JsonNode.Parse(getUrlBody);

        if (getUrlResult?["ok"]?.GetValue<bool>() != true)
        {
            var error = getUrlResult?["error"]?.GetValue<string>() ?? "unknown error";
            throw new InvalidOperationException($"Slack files.getUploadURLExternal error: {error}");
        }

        var uploadUrl = getUrlResult["upload_url"]?.GetValue<string>()
            ?? throw new InvalidOperationException("No upload_url in response.");
        var fileId = getUrlResult["file_id"]?.GetValue<string>()
            ?? throw new InvalidOperationException("No file_id in response.");

        using var streamContent = new StreamContent(content);
        var uploadResponse = await _http.PostAsync(uploadUrl, streamContent, ct);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            var uploadBody = await uploadResponse.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"File upload failed: {uploadResponse.StatusCode}: {uploadBody}");
        }

        return fileId;
    }

    public async Task DeleteMessageAsync(string channel, string messageId, CancellationToken ct = default)
    {
        var payload = new JsonObject
        {
            ["channel"] = channel,
            ["ts"] = messageId,
        };

        var content = new StringContent(
            payload.ToJsonString(),
            Encoding.UTF8,
            "application/json");

        var response = await _http.PostAsync(DeleteMessageUrl, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Slack API returned {response.StatusCode}: {body}");

        var result = JsonNode.Parse(body);
        if (result?["ok"]?.GetValue<bool>() != true)
        {
            var error = result?["error"]?.GetValue<string>() ?? "unknown error";
            throw new InvalidOperationException($"Slack API error: {error}");
        }
    }

    public async Task<MessageResult> UploadFileAsync(
        string channel, Stream content, string fileName,
        string? title = null, string? threadId = null, CancellationToken ct = default)
    {
        var channelId = await ResolveChannelId(channel, ct);
        var fileId = await UploadFileContent(content, fileName, ct);

        var completePayload = new JsonObject
        {
            ["files"] = new JsonArray(new JsonObject { ["id"] = fileId, ["title"] = title ?? fileName }),
            ["channel_id"] = channelId,
        };

        if (threadId is not null)
            completePayload["thread_ts"] = threadId;

        var completeContent = new StringContent(
            completePayload.ToJsonString(), Encoding.UTF8, "application/json");
        var completeResponse = await _http.PostAsync(CompleteUploadUrl, completeContent, ct);
        var completeBody = await completeResponse.Content.ReadAsStringAsync(ct);
        var completeResult = JsonNode.Parse(completeBody);

        if (completeResult?["ok"]?.GetValue<bool>() != true)
        {
            var error = completeResult?["error"]?.GetValue<string>() ?? "unknown error";
            throw new InvalidOperationException($"Slack files.completeUploadExternal error: {error}");
        }

        var ts = await GetFileMessageTs(fileId, channelId, ct);

        return new MessageResult
        {
            MessageId = ts,
            ThreadId = ts,
            Channel = channelId,
            Timestamp = ts.Length > 0 ? ParseSlackTimestamp(ts) : DateTimeOffset.UtcNow,
        };
    }

    private static string? ExtractTsFromFiles(JsonArray? files, string channelId)
    {
        if (files is null) return null;
        foreach (var file in files)
        {
            var shares = file?["shares"];
            if (shares is null) continue;
            foreach (var shareType in new[] { "public", "private" })
            {
                var channelShares = shares[shareType]?[channelId]?.AsArray();
                if (channelShares is { Count: > 0 })
                    return channelShares[0]?["ts"]?.GetValue<string>();
            }
        }
        return null;
    }

    private static readonly HashSet<string> PermanentErrors = ["missing_scope", "invalid_auth", "not_authed", "account_inactive", "token_revoked"];

    private async Task<string> GetFileMessageTs(string fileId, string channelId, CancellationToken ct)
    {
        for (var attempt = 0; attempt < _maxRetries; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(1000, ct);

            var response = await _http.GetAsync($"{FilesInfoUrl}?file={fileId}", ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            var result = JsonNode.Parse(body);

            if (result?["ok"]?.GetValue<bool>() != true)
            {
                var error = result?["error"]?.GetValue<string>() ?? "";
                if (PermanentErrors.Contains(error))
                    throw new InvalidOperationException($"Slack files.info error: {error}");
                continue;
            }

            var shares = result["file"]?["shares"];
            if (shares is null)
                continue;

            foreach (var shareType in new[] { "public", "private" })
            {
                var channelShares = shares[shareType]?[channelId]?.AsArray();
                if (channelShares is { Count: > 0 })
                    return channelShares[0]?["ts"]?.GetValue<string>() ?? "";
            }
        }

        return "";
    }

    private async Task<string> ResolveChannelId(string channel, CancellationToken ct)
    {
        // Already a channel ID
        if (!channel.StartsWith('#'))
            return channel;

        var name = channel.TrimStart('#');
        var cursor = "";
        do
        {
            var url = $"{ConversationsListUrl}?limit=200&exclude_archived=true&types=public_channel,private_channel";
            if (cursor.Length > 0)
                url += $"&cursor={Uri.EscapeDataString(cursor)}";

            var response = await _http.GetAsync(url, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            var result = JsonNode.Parse(body);

            if (result?["ok"]?.GetValue<bool>() != true)
            {
                var error = result?["error"]?.GetValue<string>() ?? "unknown error";
                throw new InvalidOperationException($"Slack conversations.list error: {error}");
            }

            var channels = result["channels"]?.AsArray();
            if (channels is not null)
            {
                foreach (var ch in channels)
                {
                    if (ch?["name"]?.GetValue<string>() == name)
                        return ch["id"]!.GetValue<string>();
                }
            }

            cursor = result["response_metadata"]?["next_cursor"]?.GetValue<string>() ?? "";
        } while (cursor.Length > 0);

        throw new InvalidOperationException($"Channel '{channel}' not found.");
    }

    private static DateTimeOffset ParseSlackTimestamp(string ts)
    {
        var dotIndex = ts.IndexOf('.');
        var seconds = dotIndex >= 0 ? ts[..dotIndex] : ts;
        return long.TryParse(seconds, CultureInfo.InvariantCulture, out var unix)
            ? DateTimeOffset.FromUnixTimeSeconds(unix)
            : DateTimeOffset.UtcNow;
    }

    public void Dispose() => _http.Dispose();
}
