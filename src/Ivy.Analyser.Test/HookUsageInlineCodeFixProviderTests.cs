using System.Threading.Tasks;
using Xunit;

using Verifier = Ivy.Analyser.Test.CSharpCodeFixVerifier<
    Ivy.Analyser.Analyzers.HookUsageAnalyzer,
    Ivy.Analyser.Analyzers.HookUsageInlineCodeFixProvider>;

namespace Ivy.Analyser.Test
{
    public class HookUsageInlineCodeFixProviderTests
    {
        [Fact]
        public async Task CodeFix_ExtractsInlineHook_ToVariable()
        {
            const string testCode = @"
public class TestView
{
    public object Build()
    {
        return {|#0:UseState(0)|}.Value;
    }

    private State<T> UseState<T>(T value) => default;
}

public class State<T>
{
    public T Value { get; set; }
}";

            const string fixedCode = @"
public class TestView
{
    public object Build()
    {
        var state = UseState(0);
        return state.Value;
    }

    private State<T> UseState<T>(T value) => default;
}

public class State<T>
{
    public T Value { get; set; }
}";

            var expected = Verifier.Diagnostic("IVYHOOK007")
                .WithLocation(0)
                .WithArguments("UseState");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_ExtractsHook_FromArgument()
        {
            const string testCode = @"
public class TestView
{
    public object Build()
    {
        return CreateWidget({|#0:UseEffect(42)|});
    }

    private object CreateWidget(object widget) => widget;
    private object UseEffect(int value) => null;
}";

            const string fixedCode = @"
public class TestView
{
    public object Build()
    {
        var effect = UseEffect(42);
        return CreateWidget(effect);
    }

    private object CreateWidget(object widget) => widget;
    private object UseEffect(int value) => null;
}";

            var expected = Verifier.Diagnostic("IVYHOOK007")
                .WithLocation(0)
                .WithArguments("UseEffect");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_GeneratesUniqueVariableName_WhenConflict()
        {
            const string testCode = @"
public class TestView
{
    public object Build()
    {
        var state = UseState(""existing"");
        return {|#0:UseState(0)|}.Value;
    }

    private State<T> UseState<T>(T value) => default;
}

public class State<T>
{
    public T Value { get; set; }
}";

            const string fixedCode = @"
public class TestView
{
    public object Build()
    {
        var state2 = UseState(0);
        var state = UseState(""existing"");
        return state2.Value;
    }

    private State<T> UseState<T>(T value) => default;
}

public class State<T>
{
    public T Value { get; set; }
}";

            // The second UseState call triggers both IVYHOOK005 (not at top) and IVYHOOK007 (inline)
            var expected = new[]
            {
                Verifier.Diagnostic("IVYHOOK007")
                    .WithLocation(0)
                    .WithArguments("UseState"),
            };

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
