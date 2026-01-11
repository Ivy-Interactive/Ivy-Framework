#:project ..\Ivy.Widgets.Tiptap.csproj

using Ivy;
using Ivy.Shared;
using Ivy.Widgets.Tiptap;

var server = new Server();
server.AddApp<TiptapInputView>();
await server.RunAsync();

[App]
class TiptapInputView : ViewBase
{
    public override object Build() 
    {
        var state = UseState("<p>Hello <strong>World</strong>!</p>");
        return state.ToTiptapInput();
    }
}
