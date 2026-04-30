using Ivy;
using Ivy.Plugins;
using Ivy.Plugins.Messaging;
using static Ivy.Layout;
using static Ivy.Text;

namespace MessagingHost;

[App(icon: Icons.MessageCircle, title: "Messaging Test")]
public class MessagingTestApp : ViewBase
{
    private record SentMessage(string Platform, MessageResult Result, string Preview);

    public override object? Build()
    {
        UsePluginState();
        var plugins = this.UseService<IPluginServiceProvider>();
        var channels = plugins.GetServices<IMessagingChannel>().ToList();

        var selectedPlatform = UseState(channels.FirstOrDefault()?.Platform ?? "");
        var channelName = UseState(channels.FirstOrDefault()?.DefaultChannel ?? "#ivy-plugin-test");
        var messageText = UseState("Hello from an Ivy messaging plugin!");
        var threadId = UseState("");
        var status = UseState("");
        var sending = UseState(false);
        var sentMessages = UseState<List<SentMessage>>([]);
        var fileState = UseState<FileUpload<byte[]>?>(null);

        if (channels.Count > 0 && !channels.Any(c => c.Platform == selectedPlatform.Value))
            selectedPlatform.Set(channels.First().Platform);

        var activeChannel = channels.FirstOrDefault(c => c.Platform == selectedPlatform.Value);

        var uploadContext = this.UseUpload(async (file, stream, ct) =>
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            fileState.Set(new FileUpload<byte[]>
            {
                Id = file.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Status = FileUploadStatus.Finished,
                Progress = 1,
                Content = ms.ToArray(),
            });
        });
        uploadContext.Accept("image/*");

        async Task Send(IMessagingChannel channel, MessageBuilder builder, string preview)
        {
            sending.Set(true);
            status.Set("");
            try
            {
                if (fileState.Value?.Content is not null)
                    builder.Attach(fileState.Value.Content, fileState.Value.FileName);

                var msg = builder.Build(threadId: NullIfEmpty(threadId.Value));
                var result = await channel.SendMessageAsync(channelName.Value, msg);
                sentMessages.Set([..sentMessages.Value, new SentMessage(channel.Platform, result, preview)]);
                fileState.Set(null);
                status.Set($"Sent via {channel.Platform}!");
            }
            catch (Exception ex)
            {
                status.Set($"Error: {ex.Message}");
            }
            finally
            {
                sending.Set(false);
            }
        }

        return Vertical().Gap(6).Padding(4)
            | H1("Messaging Test")
            | (channels.Count == 0
                ? new Badge("No messaging plugins loaded", BadgeVariant.Warning)
                : new Badge($"{channels.Count} channel(s): {string.Join(", ", channels.Select(c => c.Platform))}", BadgeVariant.Success))
            | new Separator()
            | (channels.Count > 1
                ? new Field(
                    selectedPlatform.ToSelectInput(channels.Select(c => c.Platform))
                  ).Label("Platform")
                : null)
            | new Field(
                channelName.ToTextInput().Placeholder("#channel or ID")
            ).Label("Channel")
            | new Field(
                messageText.ToTextInput().Placeholder("Type a message...")
            ).Label("Message")
            | new Field(
                threadId.ToTextInput().Placeholder("e.g. 1234567890.123456")
            ).Label("Thread ID")
            | new Field(
                fileState.ToFileInput(uploadContext).Placeholder("Drop an image here")
            ).Label("Image Attachment")
            | Horizontal().Gap(4)
                | new Button("Send Plain Text", onClick: async _ =>
                {
                    if (activeChannel is null || string.IsNullOrWhiteSpace(messageText.Value)) return;
                    await Send(activeChannel, new MessageBuilder().Text(messageText.Value), messageText.Value);
                }).Disabled(activeChannel is null || sending.Value)
                | new Button("Send Rich Message", onClick: async _ =>
                {
                    if (activeChannel is null) return;
                    var builder = new MessageBuilder()
                        .Bold("Title:").Text(" Example PR notification").LineBreak()
                        .Bold("Project:").Text(" :hammer_and_wrench: Framework").LineBreak()
                        .Bold("PR:").Text(" ").Link("https://github.com/Ivy-Interactive/Ivy-Framework/pull/1", "Ivy-Interactive/Ivy-Framework#1").LineBreak()
                        .Bold("Status:").Text(" ").Italic("Ready for review");
                    await Send(activeChannel, builder, "Rich: PR notification");
                }, variant: ButtonVariant.Outline).Disabled(activeChannel is null || sending.Value)
                | new Button("Send with Divider & Code", onClick: async _ =>
                {
                    if (activeChannel is null) return;
                    var builder = new MessageBuilder()
                        .Bold("Build Report")
                        .Divider()
                        .Text(":white_check_mark: All tests passed").LineBreak()
                        .Text(":package: Artifacts ready").LineBreak()
                        .Divider()
                        .CodeBlock("dotnet test --filter \"FullyQualifiedName~Ivy\"\n\nPassed! - 42 tests, 0 failed");
                    await Send(activeChannel, builder, "Rich: Build report");
                }, variant: ButtonVariant.Outline).Disabled(activeChannel is null || sending.Value)
            | (string.IsNullOrEmpty(status.Value) ? null : StatusBadge(status.Value))
            | (sentMessages.Value.Count > 0 ? SentMessagesSection(channels, sentMessages, threadId, status) : null);
    }

    private object SentMessagesSection(
        List<IMessagingChannel> channels,
        IState<List<SentMessage>> sentMessages,
        IState<string> threadId,
        IState<string> status)
    {
        return Vertical().Gap(4)
            | new Separator()
            | H2($"Sent Messages ({sentMessages.Value.Count})")
            | sentMessages.Value.Select((sent, i) =>
            {
                var channel = channels.FirstOrDefault(c => c.Platform == sent.Platform);
                return (object)(Horizontal().Gap(4)
                    | Muted($"#{i + 1}")
                    | new Badge(sent.Platform, BadgeVariant.Secondary)
                    | new Badge(sent.Preview, BadgeVariant.Outline)
                    | Muted(sent.Result.Timestamp.ToString("HH:mm:ss"))
                    | Code(sent.Result.MessageId)
                    | new Button("Reply", onClick: _ =>
                    {
                        threadId.Set(sent.Result.ThreadId);
                        return ValueTask.CompletedTask;
                    }, variant: ButtonVariant.Ghost, icon: Icons.Reply)
                    | new Button("Delete", onClick: async _ =>
                    {
                        if (channel is null) return;
                        try
                        {
                            await channel.DeleteMessageAsync(sent.Result.Channel, sent.Result.MessageId);
                            sentMessages.Set(sentMessages.Value.Where((_, idx) => idx != i).ToList());
                            status.Set("Deleted!");
                        }
                        catch (Exception ex)
                        {
                            status.Set($"Error: {ex.Message}");
                        }
                    }, variant: ButtonVariant.Ghost, icon: Icons.Trash2));
            }).ToArray();
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static object StatusBadge(string text) =>
        text.StartsWith("Error")
            ? new Badge(text, BadgeVariant.Destructive)
            : new Badge(text, BadgeVariant.Success);
}
