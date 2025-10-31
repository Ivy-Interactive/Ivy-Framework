using Ivy.Core.Hooks;

namespace Ivy.Services;


public class MemoryStreamUploadHandler(IState<byte[]?> contentState, IState<FileUpload?> uploadState, int chunkSize = 8192 /* 8KB chunks */)
    : IUploadHandler
{
    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[UploadHandler] Start reading fileId={fileUpload.Id} name='{fileUpload.FileName}' length={fileUpload.Length}");
            uploadState.Set(fileUpload);

            var totalBytes = fileUpload.Length;
            var processedBytes = 0L;
            var buffer = new byte[chunkSize];

            using var memoryStream = new MemoryStream();

            uploadState.SetStatus(FileUploadStatus.Loading);

            int bytesRead;
            var nextLog = 0.25f;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                processedBytes += bytesRead;
                var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;
                uploadState.SetProgress(progress);

                if (progress >= nextLog)
                {
                    Console.WriteLine($"[UploadHandler] Progress fileId={fileUpload.Id} {(int)(progress * 100)}%");
                    nextLog += 0.25f;
                }

                await Task.Delay(50);
            }

            contentState.Set(memoryStream.ToArray());
            uploadState.SetStatus(FileUploadStatus.Finished);
            Console.WriteLine($"[UploadHandler] Finished fileId={fileUpload.Id} bytes={processedBytes}");
        }
        catch (OperationCanceledException)
        {
            uploadState.SetStatus(FileUploadStatus.Aborted);
            Console.WriteLine($"[UploadHandler] Aborted fileId={fileUpload.Id}");
        }
        catch (Exception)
        {
            uploadState.SetStatus(FileUploadStatus.Failed);
            Console.WriteLine($"[UploadHandler] Failed fileId={fileUpload.Id}");
            throw;
        }
    }
}
