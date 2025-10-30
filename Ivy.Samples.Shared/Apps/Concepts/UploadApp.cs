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
        var selectedFile = UseState<FileUpload?>();
        var uploadedBytes = UseState<byte[]?>();

        var uploadUrl = this.UseUpload(async (fileUpload, stream, cancellationToken) =>
        {
            var uploadId = Guid.NewGuid();
            var currentFile = fileUpload with { Id = uploadId };

            try
            {
                selectedFile.Set(currentFile);

                var totalBytes = currentFile.Length;
                var processedBytes = 0L;
                var buffer = new byte[8192]; // 8KB chunks

                using var memoryStream = new MemoryStream();

                selectedFile.SetStatus(FileUploadStatus.Loading);

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    processedBytes += bytesRead;
                    var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;
                    selectedFile.SetProgress(progress);

                    //Simulate this being slower
                    await Task.Delay(50, cancellationToken);
                }

                uploadedBytes.Set(memoryStream.ToArray());
                selectedFile.SetStatus(FileUploadStatus.Finished);
            }
            catch (OperationCanceledException)
            {
                selectedFile.SetStatus(FileUploadStatus.Aborted);
            }
            catch (Exception)
            {
                selectedFile.SetStatus(FileUploadStatus.Failed);
                throw;
            }
        });

        void OnDelete(object fileId)
        {
            selectedFile.Default();
            uploadedBytes.Default();
        }

        return Layout.Vertical()
               | Text.H1("Single File Upload")
               | selectedFile.ToFileInput(uploadUrl).Accept("*/*").Placeholder("Choose a file to upload").HandleDelete(OnDelete)
               | selectedFile.ToDetails()
                   .Builder(e => e!.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                   .Builder(e => e!.Progress, e => e.Func((float x) => x.ToString("P0")))
            ;
    }
}

// public class MemoryStreamUploadHandler() : IUploadHandler
// {
//     public MemoryStreamUploadHandler(IState<byte[]?> state)
//     {
//     }
//     
//     public MemoryStreamUploadHandler(IState<ImmutableArray<byte[]?> state)
//     {
//     }
//     
//     public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
//     {
//         //todo:
//     }
// }
//
// public interface IUploadHandler
// {
//     Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken);
// }

public class MultipleFilesUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload>());
        var uploadCount = UseState(0);

        var uploadUrl = this.UseUpload(async (fileUpload, stream, cancellationToken) =>
        {
            var uploadId = Guid.NewGuid();
            var currentFile = fileUpload with { Id = uploadId };

            try
            {
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

                    //Simulate this being slower
                    await Task.Delay(50, cancellationToken);
                }

                // Mark as finished - thread-safe atomic operation
                var finishedFile = currentFile with { Status = FileUploadStatus.Finished };
                selectedFiles.Set(files => files.Replace(currentFile, finishedFile));

                uploadCount.Set(count => count + 1);
            }
            catch (OperationCanceledException)
            {
                // Upload was aborted by user
                var abortedFile = currentFile with { Status = FileUploadStatus.Aborted };
                selectedFiles.Set(files => files.Replace(currentFile, abortedFile));
            }
        });

        void OnDelete(object fileId)
        {
            var file = selectedFiles.Value.FirstOrDefault(f => f.Id.Equals(fileId));
            if (file == null) return;
            selectedFiles.Set(files => files.Remove(file));
        }

        var layout = Layout.Vertical()
                     | Text.H1("Multiple Files Upload")
                     | selectedFiles.ToFileInput(uploadUrl).Accept("*/*").Placeholder("Choose files to upload").HandleDelete(OnDelete)
                     | selectedFiles.Value.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                         .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                         .Remove(e => e.Id)
                     | (uploadCount.Value > 0 ? Text.Block($"Uploaded {uploadCount.Value} file(s)") : null);


        return layout;
    }
}
