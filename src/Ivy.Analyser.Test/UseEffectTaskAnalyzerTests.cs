using System.Threading.Tasks;
using Xunit;
using VerifyCS = Ivy.Analyser.Test.CSharpAnalyzerVerifier<
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

    [Fact]
    public async Task ContinueWith_InsideUseEffect_ReportsDiagnostic()
    {
        var test = Stubs + @"
public class MyView : Ivy.ViewBase
{
    public override object Build()
    {
        this.UseEffect(() => {
            {|IVYEFFECT001:Task.Delay(300).ContinueWith(_ => { })|};
            return (IDisposable)null;
        });
        return null;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
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
            {|IVYEFFECT001:Task.Delay(300).ContinueWith(_ => { })|};
            return (IDisposable)null;
        }, x);
        return null;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
