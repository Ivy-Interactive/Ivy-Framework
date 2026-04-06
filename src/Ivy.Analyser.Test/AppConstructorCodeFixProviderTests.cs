using System.Threading.Tasks;
using Xunit;

using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Ivy.Analyser.Analyzers.AppConstructorAnalyzer,
    Ivy.Analyser.Analyzers.AppConstructorCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Ivy.Analyser.Test
{
    public class AppConstructorCodeFixProviderTests
    {
        [Fact]
        public async Task CodeFix_AddsParameterlessConstructor_ToAppWithParameterizedConstructor()
        {
            const string testCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class {|#0:MyApp|}
    {
        public MyApp(string param) { }

        public object Build() => null;
    }
}";

            const string fixedCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class MyApp
    {
        public MyApp()
        {
        }

        public MyApp(string param) { }

        public object Build() => null;
    }
}";

            var expected = Verifier.Diagnostic(Analyzers.AppConstructorAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("MyApp");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_AddsConstructor_BetweenFieldsAndMethods()
        {
            const string testCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class {|#0:MyApp|}
    {
        private int _field = 0;

        public MyApp(int value) { _field = value; }

        public object Build() => null;
    }
}";

            const string fixedCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class MyApp
    {
        private int _field = 0;

        public MyApp()
        {
        }

        public MyApp(int value) { _field = value; }

        public object Build() => null;
    }
}";

            var expected = Verifier.Diagnostic(Analyzers.AppConstructorAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("MyApp");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_HandlesClassWithOnlyConstructor()
        {
            const string testCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class {|#0:MyApp|}
    {
        public MyApp(string param) { }
    }
}";

            const string fixedCode = @"
namespace Ivy
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AppAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [Ivy.App]
    public class MyApp
    {
        public MyApp()
        {
        }

        public MyApp(string param) { }
    }
}";

            var expected = Verifier.Diagnostic(Analyzers.AppConstructorAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("MyApp");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
