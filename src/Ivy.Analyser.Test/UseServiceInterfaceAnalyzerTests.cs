using System.Threading.Tasks;
using Xunit;
using VerifyCS = Ivy.Analyser.Test.CSharpAnalyzerVerifier<
    Ivy.Analyser.Analyzers.UseServiceInterfaceAnalyzer>;

namespace Ivy.Analyser.Test;

public class UseServiceInterfaceAnalyzerTests
{
    private const string Stubs = @"
using System;
using TestServices;

namespace Ivy
{
    public abstract class ViewBase
    {
        public abstract object Build();
        protected T UseService<T>() { throw new NotImplementedException(); }
    }
}

namespace TestServices
{
    public interface IConfigService { }
    public class ConfigService : IConfigService { }

    public interface IJobService { }
    public class JobService : IJobService { }

    public class NoInterfaceService { }
}
";

    [Fact]
    public async Task ConcreteType_WithInterface_ReportsWarning()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<{|IVYSERVICE001:ConfigService|}>();
        return new object();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task InterfaceType_NoDiagnostic()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        return new object();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ConcreteType_WithoutInterface_NoDiagnostic()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var svc = UseService<NoInterfaceService>();
        return new object();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ThisQualified_ConcreteType_ReportsWarning()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var job = this.UseService<{|IVYSERVICE001:JobService|}>();
        return new object();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NonBuildMethod_StillChecked()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build() { return new object(); }

    public void SomeMethod()
    {
        var config = UseService<{|IVYSERVICE001:ConfigService|}>();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleViolations_ReportsAll()
    {
        var test = Stubs + @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<{|IVYSERVICE001:ConfigService|}>();
        var job = UseService<{|IVYSERVICE001:JobService|}>();
        return new object();
    }
}";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
