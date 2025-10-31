using System.Collections.Immutable;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Tables;
using Microsoft.Extensions.Logging;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Upload, searchHints: ["file", "attachment", "upload", "stream", "progress", "multipart"])]
public class UploadApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Single File", new SingleFileUpload()),
            new Tab("Multiple Files", new MultipleFilesUpload())
        );
    }
}

public class SingleFileUpload : ViewBase
{
    public override object? Build()
    {
        var uploadState = UseState<FileUpload<byte[]>?>();
        var uploadHandler = new MemoryStreamUploadHandler(uploadState);
        var uploadContext = this.UseUpload(uploadHandler);

        return Layout.Vertical()
               | Text.H1("Single File Upload")
               | uploadState.ToFileInput(uploadContext).Accept("*/*").Placeholder("Choose a file to upload")
               | uploadState.ToDetails()
                   .Remove(e => e!.Content!)
                   .Builder(e => e!.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                   .Builder(e => e!.Progress, e => e.Func((float x) => x.ToString("P0")))
            ;
    }
}

public class MultipleFilesUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload>());
        var uploadCount = UseState(0);
        var upload = this.UseUpload(async (fileUpload, stream, cancellationToken) =>
        {
            var currentFile = fileUpload;

            try
            {
                Console.WriteLine($"[Samples] Start multi upload fileId={currentFile.Id} name='{currentFile.FileName}' length={currentFile.Length}");
                selectedFiles.Set(files => files.Add(currentFile));

                var totalBytes = currentFile.Length;
                var processedBytes = 0L;
                var buffer = new byte[8192]; // 8KB chunks

                using var memoryStream = new MemoryStream();

                // Update to Loading state
                var loadingFile = currentFile with { Status = FileUploadStatus.Loading };
                selectedFiles.Set(files => files.Replace(currentFile, loadingFile));
                currentFile = loadingFile;

                int bytesRead;
                var nextLog = 0.25f;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    processedBytes += bytesRead;
                    var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;

                    // Update progress - thread-safe atomic operation
                    var updatedFile = currentFile with { Progress = progress };
                    selectedFiles.Set(files => files.Replace(currentFile, updatedFile));
                    currentFile = updatedFile;

                    if (progress >= nextLog)
                    {
                        Console.WriteLine($"[Samples] Progress fileId={currentFile.Id} {(int)(progress * 100)}%");
                        nextLog += 0.25f;
                    }

                    //Simulate this being slower
                    await Task.Delay(50, cancellationToken);
                }

                // Mark as finished - thread-safe atomic operation
                var finishedFile = currentFile with { Status = FileUploadStatus.Finished };
                selectedFiles.Set(files => files.Replace(currentFile, finishedFile));

                uploadCount.Set(count => count + 1);
                Console.WriteLine($"[Samples] Finished fileId={currentFile.Id}");
            }
            catch (OperationCanceledException)
            {
                // Upload was aborted by user
                var abortedFile = currentFile with { Status = FileUploadStatus.Aborted };
                selectedFiles.Set(files => files.Replace(currentFile, abortedFile));
                Console.WriteLine($"[Samples] Aborted fileId={currentFile.Id}");
            }
        });

        var layout = Layout.Vertical()
                     | Text.H1("Multiple Files Upload")
                     | selectedFiles.ToFileInput(upload).Accept("*/*").Placeholder("Choose files to upload")
                     | selectedFiles.Value.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                         .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                         .Remove(e => e.Id)
                     | (uploadCount.Value > 0 ? Text.Block($"Uploaded {uploadCount.Value} file(s)") : null);


        return layout;
    }
}
