namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.BellRing, searchHints: ["notifications", "messages", "feedback", "snackbar", "alerts", "popup"])]
public class ToastApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            Layout.Horizontal(
                new Button("Default Toast", _ => client.Toast("This is a standard default toast message.")),
                new Button("Destructive Toast", _ => client.Toast("An error occurred during the operation.", "Error").Destructive()),
                new Button("Success Toast", _ => client.Toast("Record saved successfully.", "Success").Success()),
                new Button("Warning Toast", _ => client.Toast("Your session is about to expire.", "Warning").Warning()),
                new Button("Info Toast", _ => client.Toast("A new update is available for download.", "Info").Info())
            ).Wrap(),

            new Separator(),
            Text.H3("Edge Cases"),
            Layout.Horizontal(
                new Button("Long Description", _ => client.Toast("This is a very long description that might wrap multiple lines. It is important to test how the browser and the toast component handles long strings of text so we can ensure the UI doesn't break or look distorted when displaying verbose error or information messages from the backend.", "Verbose Information").Info()),
                new Button("No Title", _ => client.Toast("This toast has no title, only a description.").Warning()),
                new Button("Exception Toast", _ =>
                {
                    try
                    {
                        throw new InvalidOperationException("Simulated exception message with a stack trace.");
                    }
                    catch (Exception ex)
                    {
                        client.Toast(ex).Destructive();
                    }
                })
            ).Wrap()
        );
    }
}
