using System.Collections.Immutable;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Views.Forms;
using Ivy.Views.Tables;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Upload, path: ["Concepts"], searchHints: ["form", "upload", "validation", "blocking"])]
public class FormUploadBlockingDemo : ViewBase
{
    public class DocumentSubmission
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public FileUpload<byte[]>? Document { get; set; }
        public ImmutableArray<FileUpload<byte[]>> Attachments { get; set; } = ImmutableArray<FileUpload<byte[]>>.Empty;
    }

    public override object? Build()
    {
        var model = UseState(new DocumentSubmission());

        var form = model.ToForm()
            .Builder(e => e.Title, state => state.ToTextInput().Placeholder("Enter document title"))
            .Builder(e => e.Description, state => state.ToTextInput().Placeholder("Enter description").Variant(TextInputs.Textarea))
            .Builder(e => e.Document, (state, context) =>
            {
                var upload = context.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .Accept(".pdf,.docx")
                    .MaxFileSize(10 * 1024 * 1024); // 10 MB
                return state.ToFileInput(upload).Placeholder("Upload main document");
            })
            .Builder(e => e.Attachments, (state, context) =>
            {
                var upload = context.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .MaxFiles(3)
                    .MaxFileSize(5 * 1024 * 1024); // 5 MB
                return state.ToFileInput(upload).Placeholder("Upload attachments (max 3)");
            });

        return Layout.Vertical().Gap(4)
               | Text.H1("Form Upload Blocking Demo")
               | Callout.Info("This demo shows how the form automatically blocks submission while files are uploading. Try uploading large files and notice the submit button becomes disabled until uploads complete.")
               | form
               | (model.Value.Document != null
                   ? Text.P($"Main document: {model.Value.Document.FileName} - {Utils.FormatBytes(model.Value.Document.Length)}")
                   : null)
               | (model.Value.Attachments.Length > 0
                   ? Text.P($"Attachments: {model.Value.Attachments.Length} file(s) uploaded")
                   : null);
    }
}
