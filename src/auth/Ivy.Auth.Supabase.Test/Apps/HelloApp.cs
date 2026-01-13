using Ivy.Apps;
using Ivy.Core;

namespace Ivy.Auth.Supabase.Test.Apps;

[App]
public class HelloApp : ViewBase
{
    public override object? Build()
    {
        return "You are authenticated.";
    }
}