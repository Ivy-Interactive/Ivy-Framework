using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Ivy.Analyser.Tests
{
    public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public static DiagnosticResult Diagnostic(string diagnosticId) =>
            new DiagnosticResult(diagnosticId, DiagnosticSeverity.Warning);

        public static async Task VerifyAnalyzerAsync(string source)
        {
            var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
            {
                TestCode = source,
            };
            await test.RunAsync();
        }

        public static async Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
        {
            var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };
            test.ExpectedDiagnostics.Add(expected);
            await test.RunAsync();
        }

        public static async Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        {
            var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource,
                // BatchFixer applies one fix per iteration, so we need as many iterations as diagnostics
                NumberOfFixAllIterations = expected.Length,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }
    }
}
