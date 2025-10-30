using Ivy.Core.Hooks;

namespace Ivy.Services;

public class MultiFileUploadHandler : IUploadHandler
{
    public Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class MemoryStreamUploadHandler(IState<byte[]?> contentState, IState<FileUpload?> uploadState, int chunkSize = 8192 /* 8KB chunks */)
    : IUploadHandler
{
    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            uploadState.Set(fileUpload);

            var totalBytes = fileUpload.Length;
            var processedBytes = 0L;
            var buffer = new byte[chunkSize];

            using var memoryStream = new MemoryStream();

            uploadState.SetStatus(FileUploadStatus.Loading);

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                processedBytes += bytesRead;
                var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;
                uploadState.SetProgress(progress);
            }

            contentState.Set(memoryStream.ToArray());
            uploadState.SetStatus(FileUploadStatus.Finished);
        }
        catch (OperationCanceledException)
        {
            uploadState.SetStatus(FileUploadStatus.Aborted);
        }
        catch (Exception)
        {
            uploadState.SetStatus(FileUploadStatus.Failed);
            throw;
        }
    }
}