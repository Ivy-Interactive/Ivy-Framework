using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HookUsageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IVYHOOK001";
        private const string Title = "Invalid Ivy Hook Usage";
        private const string MessageFormat = "Ivy hook '{0}' can only be used directly inside the Build() method";
        private const string Description = "Ivy hooks must be called directly inside the Build() method, not inside lambdas, local functions, or other methods.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        private static readonly ImmutableHashSet<string> HookNames = ImmutableHashSet.Create(
            "UseState",
            "UseEffect",
            "UseMemo",
            "UseRef",
            "UseContext",
            "UseCallback"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Get the method name
            var methodName = GetMethodName(invocation);
            if (methodName == null || !HookNames.Contains(methodName))
            {
                return;
            }

            // Check if the invocation is valid
            if (!IsValidHookUsage(invocation))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static string? GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is IdentifierNameSyntax memberIdentifier)
            {
                return memberIdentifier.Identifier.Text;
            }

            return null;
        }

        private static bool IsValidHookUsage(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                // Check for invalid contexts
                if (current is LambdaExpressionSyntax ||
                    current is LocalFunctionStatementSyntax ||
                    current is AnonymousMethodExpressionSyntax)
                {
                    return false;
                }

                // Check if we're in a method declaration
                if (current is MethodDeclarationSyntax method)
                {
                    // Must be in Build() method
                    return method.Identifier.Text == "Build";
                }

                current = current.Parent;
            }

            // Not in any method
            return false;
        }
    }
}