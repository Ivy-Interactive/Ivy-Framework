using Ivy;
using Ivy.Widgets;

namespace Ivy.Auth.Firebase.Test.Apps;

[App("Firebase Auth Test", "/")]
public class FirebaseHelloApp : IView
{
    public void Render()
    {
        TextBlock("Hello from Firebase Auth Test!");
        
        if (Context.User == null)
        {
            TextBlock("Not logged in");
        }
        else
        {
            TextBlock($"Logged in as: {Context.User.Email}");
            TextBlock($"Display name: {Context.User.DisplayName}");
        }
    }
}