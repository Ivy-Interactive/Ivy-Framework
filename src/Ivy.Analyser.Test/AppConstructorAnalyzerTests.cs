using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Ivy.Analyser.Analyzers.AppConstructorAnalyzer>;

namespace Ivy.Analyser.Test;

public class AppConstructorAnalyzerTests
{
    private const string Stubs = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute
    {
        public AppAttribute(string id = null, string title = null) { }
    }

    public abstract class ViewBase
    {
        public abstract object Build();
    }

    public abstract class AppBase : ViewBase { }
}
";

    [Fact]
    public async Task AppWithParameterlessConstructor_ViewBase_NoDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class MyApp : Ivy.ViewBase
{
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithParameterlessConstructor_AppBase_NoDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class MyApp : Ivy.AppBase
{
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithParameterizedConstructor_ViewBase_ReportsDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class {|IVYAPP001:MyApp|} : Ivy.ViewBase
{
    private readonly object _svc;
    public MyApp(object svc) { _svc = svc; }
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithParameterizedConstructor_AppBase_ReportsDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class {|IVYAPP001:MyApp|} : Ivy.AppBase
{
    private readonly object _svc;
    public MyApp(object svc) { _svc = svc; }
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithPrimaryConstructor_ViewBase_ReportsDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class {|IVYAPP001:MyApp|}(object svc) : Ivy.ViewBase
{
    public override object Build() => svc;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithPrimaryConstructor_AppBase_ReportsDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class {|IVYAPP001:MyApp|}(object svc) : Ivy.AppBase
{
    public override object Build() => svc;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithAllOptionalParams_NoDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class MyApp : Ivy.ViewBase
{
    public MyApp(int x = 0, string y = null) { }
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppWithBothParameterlessAndParameterized_NoDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public class MyApp : Ivy.ViewBase
{
    public MyApp() { }
    public MyApp(object svc) { }
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AbstractAppWithParameterizedConstructor_NoDiagnostic()
    {
        var test = Stubs + @"
[Ivy.App]
public abstract class MyAppBase : Ivy.ViewBase
{
    protected MyAppBase(object svc) { }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ClassWithoutAppAttribute_ParameterizedConstructor_NoDiagnostic()
    {
        var test = Stubs + @"
public class NotAnApp : Ivy.ViewBase
{
    public NotAnApp(object svc) { }
    public override object Build() => null;
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
