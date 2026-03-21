using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseEffectTaskAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "IVYEFFECT001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Avoid Task.ContinueWith inside UseEffect",
        "Task.ContinueWith() inside UseEffect can cause NullReferenceException when the component is disposed. Use a CancellationTokenSource with proper cleanup instead. See: https://docs.ivy.app/other/ivy/analyser/ivyeffect001.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Task.ContinueWith() inside UseEffect creates a fire-and-forget continuation that runs on a thread pool thread with no component lifecycle awareness, causing NullReferenceException when calling .Set() on disposed state.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a .ContinueWith(...) call
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => null
        };

        if (methodName != "ContinueWith")
            return;

        // Walk up the syntax tree to check if we're inside a UseEffect lambda
        if (IsInsideUseEffectCallback(invocation))
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsInsideUseEffectCallback(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            // Check if we hit a lambda/anonymous method that is an argument to UseEffect
            if (current is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax)
            {
                if (current.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax parentInvocation } })
                {
                    if (IsUseEffectCall(parentInvocation))
                        return true;
                }
            }

            // Stop walking at method declarations — we've left any lambda scope
            if (current is MethodDeclarationSyntax)
                return false;

            current = current.Parent;
        }

        return false;
    }

    private static bool IsUseEffectCall(InvocationExpressionSyntax invocation)
    {
        var name = invocation.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                _ => null
            },
            _ => null
        };

        return name == "UseEffect";
    }
}
