using Ivy.Analyser.Analyzers;
using Xunit;

using Verifier = Ivy.Analyser.Tests.CSharpCodeFixVerifier<
    Ivy.Analyser.Analyzers.UseServiceInterfaceAnalyzer,
    Ivy.Analyser.Analyzers.UseServiceInterfaceCodeFixProvider>;

namespace Ivy.Analyser.Tests
{
    public class UseServiceInterfaceCodeFixProviderTests
    {
        [Fact]
        public async Task CodeFix_ReplacesConcrete_WithInterface()
        {
            const string testCode = @"
namespace TestNamespace
{
    public interface IConfigService { }
    public class ConfigService { }

    public class TestClass
    {
        public void TestMethod()
        {
            var config = UseService<{|#0:ConfigService|}>();
        }

        private T UseService<T>() => default;
    }
}";

            const string fixedCode = @"
namespace TestNamespace
{
    public interface IConfigService { }
    public class ConfigService { }

    public class TestClass
    {
        public void TestMethod()
        {
            var config = UseService<IConfigService>();
        }

        private T UseService<T>() => default;
    }
}";

            var expected = Verifier.Diagnostic(UseServiceInterfaceAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("ConfigService");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_NotOffered_WhenInterfaceDoesNotExist()
        {
            const string testCode = @"
namespace TestNamespace
{
    // No IConfigService interface exists
    public class ConfigService { }

    public class TestClass
    {
        public void TestMethod()
        {
            var config = UseService<ConfigService>();
        }

        private T UseService<T>() => default;
    }
}";

            // No diagnostic expected because interface doesn't exist
            await Verifier.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CodeFix_WorksWithMemberAccess()
        {
            const string testCode = @"
namespace TestNamespace
{
    public interface IJobService { }
    public class JobService { }

    public class ServiceProvider
    {
        public T UseService<T>() => default;
    }

    public class TestClass
    {
        private ServiceProvider provider = new ServiceProvider();

        public void TestMethod()
        {
            var job = provider.UseService<{|#0:JobService|}>();
        }
    }
}";

            const string fixedCode = @"
namespace TestNamespace
{
    public interface IJobService { }
    public class JobService { }

    public class ServiceProvider
    {
        public T UseService<T>() => default;
    }

    public class TestClass
    {
        private ServiceProvider provider = new ServiceProvider();

        public void TestMethod()
        {
            var job = provider.UseService<IJobService>();
        }
    }
}";

            var expected = Verifier.Diagnostic(UseServiceInterfaceAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("JobService");

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task CodeFix_FixAll_HandlesMultipleViolations()
        {
            const string testCode = @"
namespace TestNamespace
{
    public interface IConfigService { }
    public class ConfigService { }

    public interface IJobService { }
    public class JobService { }

    public class TestClass
    {
        public void TestMethod()
        {
            var config = UseService<{|#0:ConfigService|}>();
            var job = UseService<{|#1:JobService|}>();
        }

        private T UseService<T>() => default;
    }
}";

            const string fixedCode = @"
namespace TestNamespace
{
    public interface IConfigService { }
    public class ConfigService { }

    public interface IJobService { }
    public class JobService { }

    public class TestClass
    {
        public void TestMethod()
        {
            var config = UseService<IConfigService>();
            var job = UseService<IJobService>();
        }

        private T UseService<T>() => default;
    }
}";

            var expected = new[]
            {
                Verifier.Diagnostic(UseServiceInterfaceAnalyzer.DiagnosticId)
                    .WithLocation(0)
                    .WithArguments("ConfigService"),
                Verifier.Diagnostic(UseServiceInterfaceAnalyzer.DiagnosticId)
                    .WithLocation(1)
                    .WithArguments("JobService")
            };

            await Verifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
