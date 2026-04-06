using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Ivy.Analyser.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

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

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(Stubs + source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        };

        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            references.Append(runtimeRef),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new UseServiceInterfaceAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    [Fact]
    public async Task ConcreteType_WithInterface_ReportsWarning()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<ConfigService>();
        return new object();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal(UseServiceInterfaceAnalyzer.DiagnosticId, diagnostics[0].Id);
        Assert.Contains("ConfigService", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task InterfaceType_NoDiagnostic()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        return new object();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ConcreteType_WithoutInterface_NoDiagnostic()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var svc = UseService<NoInterfaceService>();
        return new object();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ThisQualified_ConcreteType_ReportsWarning()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var job = this.UseService<JobService>();
        return new object();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal(UseServiceInterfaceAnalyzer.DiagnosticId, diagnostics[0].Id);
        Assert.Contains("JobService", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task NonBuildMethod_StillChecked()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build() { return new object(); }

    public void SomeMethod()
    {
        var config = UseService<ConfigService>();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal(UseServiceInterfaceAnalyzer.DiagnosticId, diagnostics[0].Id);
    }

    [Fact]
    public async Task MultipleViolations_ReportsAll()
    {
        var source = @"
class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var config = UseService<ConfigService>();
        var job = UseService<JobService>();
        return new object();
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, d => Assert.Equal(UseServiceInterfaceAnalyzer.DiagnosticId, d.Id));
    }
}
