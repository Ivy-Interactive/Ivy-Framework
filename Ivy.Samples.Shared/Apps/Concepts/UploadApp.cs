using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;
using Microsoft.Extensions.Logging;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Upload, searchHints: ["file", "attachment", "upload", "stream", "progress", "multipart"])]
public class UploadApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedFile = UseState<FileInput?>();
        var uploadedBytes = UseState<byte[]?>();

        var uploadUrl = this.UseUpload(async (fileUpload) =>
        {
            try
            {
                if (selectedFile.Value == null)
                {
                    selectedFile.Set(new FileInput(fileUpload));
                }

                var totalBytes = fileUpload.Length;
                var processedBytes = 0L;
                var buffer = new byte[8192]; // 8KB chunks

                using var memoryStream = new MemoryStream();

                selectedFile.SetState(FileInputState.Loading);

                int bytesRead;
                while ((bytesRead = await fileUpload.Stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    processedBytes += bytesRead;
                    var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;
                    selectedFile.SetProgress(progress);
                }

                uploadedBytes.Set(memoryStream.ToArray());
            }
            catch (Exception)
            {
                selectedFile.SetState(FileInputState.Failed);
                throw;
            }
            finally
            {
                selectedFile.SetState(FileInputState.Finished);
            }
        });

        return Layout.Vertical()
               | selectedFile.ToFileInput(uploadUrl).Accept("*/*").Placeholder("Choose a file to upload")
               | selectedFile
            ;
    }
}
