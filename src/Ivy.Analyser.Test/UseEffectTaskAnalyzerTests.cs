using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Ivy.Analyser.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Ivy.Analyser.Analyzers.UseEffectTaskAnalyzer>;

namespace Ivy.Analyser.Test;

public class UseEffectTaskAnalyzerTests
{
    private const string Stubs = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy
{
    public abstract class ViewBase
    {
        public abstract object Build();
    }
}

static class UseEffectExtensions
{
    public static void UseEffect(this Ivy.ViewBase view, Func<IDisposable> callback, params object[] deps) { }
    public static void UseEffect(this Ivy.ViewBase view, Func<IAsyncDisposable> callback, params object[] deps) { }
}
";

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.IAsyncDisposable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
        };

        // Add runtime assembly references
        var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(System.IO.Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            references.Append(runtimeRef),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new UseEffectTaskAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    [Fact]
    public async Task ContinueWith_InsideUseEffect_ReportsDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        this.UseEffect(() => {
            Task.Delay(300).ContinueWith(_ => { });
            return null;
        });
        return null;
    }
}
";
        var diagnostics = await GetDiagnosticsAsync(test);
        var ivyDiag = diagnostics.Where(d => d.Id == "IVYEFFECT001").ToArray();
        Assert.Single(ivyDiag);
        Assert.Equal(DiagnosticSeverity.Warning, ivyDiag[0].Severity);
    }

    [Fact]
    public async Task ContinueWith_OutsideUseEffect_NoDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        Task.Delay(300).ContinueWith(_ => { });
        return null;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ProperCancellationPattern_InsideUseEffect_NoDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        this.UseEffect(() => {
            var cts = new CancellationTokenSource();
            return cts;
        });
        return null;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ContinueWith_InNonUseEffectLambda_NoDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        System.Action a = () => {
            Task.Delay(300).ContinueWith(_ => { });
        };
        return null;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ContinueWith_InsideUseEffect_WithDeps_ReportsDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        var x = 1;
        this.UseEffect(() => {
            Task.Delay(300).ContinueWith(_ => { });
            return null;
        }, x);
        return null;
    }
}
";
        var diagnostics = await GetDiagnosticsAsync(test);
        var ivyDiag = diagnostics.Where(d => d.Id == "IVYEFFECT001").ToArray();
        Assert.Single(ivyDiag);
    }
}
