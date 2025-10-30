using Ivy.Hooks;
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
        var selectedFile = UseState<FileInput?>();
        var uploadedBytes = UseState<byte[]?>();

        var uploadUrl = this.UseUpload(async (fileUpload) =>
        {
            try
            {
                selectedFile.Set(new FileInput(fileUpload));

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

                    //Simulate this being slower
                    await Task.Delay(50);
                }

                uploadedBytes.Set(memoryStream.ToArray());
                selectedFile.SetState(FileInputState.Finished);
            }
            catch (Exception)
            {
                selectedFile.SetState(FileInputState.Failed);
                throw;
            }
        });

        void OnClear()
        {
            selectedFile.Default();
            uploadedBytes.Default();
        }

        return Layout.Vertical()
               | Text.H1("Single File Upload")
               | selectedFile.ToFileInput(uploadUrl).Accept("*/*").Placeholder("Choose a file to upload").HandleClear(OnClear)
               | selectedFile.ToDetails()
            ;
    }
}

public class MultipleFilesUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState<IEnumerable<FileInput>?>();
        var uploadCount = UseState(0);

        var uploadUrl = this.UseUpload(async (fileUpload) =>
        {
            var newFile = new FileInput(fileUpload);

            // Add the new file to the list
            var currentFiles = selectedFiles.Value?.ToList() ?? new List<FileInput>();
            currentFiles.Add(newFile);
            selectedFiles.Set(currentFiles);

            // Find the file we just added to update its progress
            var fileToUpdate = currentFiles.Last();
            var fileIndex = currentFiles.Count - 1;

            var totalBytes = fileUpload.Length;
            var processedBytes = 0L;
            var buffer = new byte[8192]; // 8KB chunks

            using var memoryStream = new MemoryStream();

            // Update state for this specific file
            currentFiles[fileIndex] = fileToUpdate with { State = FileInputState.Loading };
            selectedFiles.Set(currentFiles.ToArray());

            int bytesRead;
            while ((bytesRead = await fileUpload.Stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                processedBytes += bytesRead;
                var progress = totalBytes > 0 ? ((float)processedBytes / totalBytes) : 0;

                // Update progress for this specific file
                var updatedFiles = currentFiles.ToArray();
                updatedFiles[fileIndex] = updatedFiles[fileIndex] with { Progress = progress };
                selectedFiles.Set(updatedFiles);

                //Simulate this being slower
                await Task.Delay(50);
            }

            // Mark as finished
            var finalFiles = selectedFiles.Value!.ToArray();
            finalFiles[fileIndex] = finalFiles[fileIndex] with { State = FileInputState.Finished };
            selectedFiles.Set(finalFiles);

            uploadCount.Set(uploadCount.Value + 1);
        });

        void OnClear()
        {
            selectedFiles.Default();
            uploadCount.Default();
        }

        var layout = Layout.Vertical()
                     | Text.H1("Multiple Files Upload")
                     | selectedFiles.ToFileInput(uploadUrl).Accept("*/*").Placeholder("Choose files to upload").HandleClear(OnClear)
                     | selectedFiles.Value?.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                         .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                         .Remove(e => e.Id)
                     | (uploadCount.Value > 0 ? Text.Block($"Uploaded {uploadCount.Value} file(s)") : null);


        return layout;
    }
}
