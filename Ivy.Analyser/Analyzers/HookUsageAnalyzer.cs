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

        public const string DiagnosticIdConditional = "IVYHOOK002";
        private const string TitleConditional = "Ivy Hook Called Conditionally";
        private const string MessageFormatConditional = "Ivy hook '{0}' cannot be called conditionally. Hooks must be called in the same order on every render.";
        private const string DescriptionConditional = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside if statements, ternary operators, or other conditional logic.";

        public const string DiagnosticIdLoop = "IVYHOOK003";
        private const string TitleLoop = "Ivy Hook Called in Loop";
        private const string MessageFormatLoop = "Ivy hook '{0}' cannot be called inside a loop. Hooks must be called in the same order on every render.";
        private const string DescriptionLoop = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside for, foreach, while, or do-while loops.";

        public const string DiagnosticIdSwitch = "IVYHOOK004";
        private const string TitleSwitch = "Ivy Hook Called in Switch Statement";
        private const string MessageFormatSwitch = "Ivy hook '{0}' cannot be called inside a switch statement. Hooks must be called in the same order on every render.";
        private const string DescriptionSwitch = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside switch statements.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        private static readonly DiagnosticDescriptor RuleConditional = new DiagnosticDescriptor(
            DiagnosticIdConditional,
            TitleConditional,
            MessageFormatConditional,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionConditional);

        private static readonly DiagnosticDescriptor RuleLoop = new DiagnosticDescriptor(
            DiagnosticIdLoop,
            TitleLoop,
            MessageFormatLoop,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionLoop);

        private static readonly DiagnosticDescriptor RuleSwitch = new DiagnosticDescriptor(
            DiagnosticIdSwitch,
            TitleSwitch,
            MessageFormatSwitch,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionSwitch);

        private static readonly ImmutableHashSet<string> HookNames = ImmutableHashSet.Create(
            "UseState",
            "UseEffect",
            "UseMemo",
            "UseRef",
            "UseContext",
            "UseCallback",
            "UseReducer",
            "UseStatic",
            "UseSignal",
            "UseTrigger",
            "UseService",
            "UseArgs"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Rule,
            RuleConditional,
            RuleLoop,
            RuleSwitch);

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

            // First check if hook is in Build() method and not in lambdas/local functions
            if (!IsValidHookUsage(invocation))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
                return; // Don't check for other violations if already invalid
            }

            // Check for conditional usage (if statements, ternary operators)
            if (IsInConditionalStatement(invocation))
            {
                var diagnostic = Diagnostic.Create(RuleConditional, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }

            // Check for loop usage
            if (IsInLoop(invocation))
            {
                var diagnostic = Diagnostic.Create(RuleLoop, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }

            // Check for switch statement usage
            if (IsInSwitchStatement(invocation))
            {
                var diagnostic = Diagnostic.Create(RuleSwitch, invocation.GetLocation(), methodName);
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

        private static bool IsInConditionalStatement(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                // Check for if statements
                if (current is IfStatementSyntax)
                {
                    return true;
                }

                // Check for ternary operators (conditional expressions)
                if (current is ConditionalExpressionSyntax)
                {
                    return true;
                }

                // Check for try-catch blocks (conditional execution)
                if (current is TryStatementSyntax)
                {
                    return true;
                }

                // Check for catch clauses
                if (current is CatchClauseSyntax)
                {
                    return true;
                }

                // Check for using statements (conditional execution)
                if (current is UsingStatementSyntax)
                {
                    return true;
                }

                // Stop checking if we reach the Build() method
                if (current is MethodDeclarationSyntax method && method.Identifier.Text == "Build")
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsInLoop(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                // Check for various loop types
                if (current is ForStatementSyntax ||
                    current is ForEachStatementSyntax ||
                    current is WhileStatementSyntax ||
                    current is DoStatementSyntax)
                {
                    return true;
                }

                // Stop checking if we reach the Build() method
                if (current is MethodDeclarationSyntax method && method.Identifier.Text == "Build")
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsInSwitchStatement(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                // Check for switch statements
                if (current is SwitchStatementSyntax)
                {
                    return true;
                }

                // Stop checking if we reach the Build() method
                if (current is MethodDeclarationSyntax method && method.Identifier.Text == "Build")
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }
    }
}