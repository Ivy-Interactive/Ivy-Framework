using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Ivy.Analyser.Test
{
    public static class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            var analyzer = new TAnalyzer();
            foreach (var descriptor in analyzer.SupportedDiagnostics)
            {
                if (descriptor.Id == diagnosticId)
                    return new DiagnosticResult(descriptor);
            }

            return new DiagnosticResult(diagnosticId, DiagnosticSeverity.Warning);
        }

        public static async Task VerifyAnalyzerAsync(string source)
        {
            var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            await test.RunAsync();
        }
    }
}
