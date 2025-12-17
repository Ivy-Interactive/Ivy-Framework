using System.Collections.Generic;
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

        public const string DiagnosticIdNotAtTop = "IVYHOOK005";
        private const string TitleNotAtTop = "Ivy Hook Not at Top of Build Method";
        private const string MessageFormatNotAtTop = "Ivy hook '{0}' must be called at the top of the Build() method, before any other statements";
        private const string DescriptionNotAtTop = "All hooks must be called at the very top of the Build() method, before any other non-hook statements. This ensures hooks are called in a consistent order on every render.";

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

        private static readonly DiagnosticDescriptor RuleNotAtTop = new DiagnosticDescriptor(
            DiagnosticIdNotAtTop,
            TitleNotAtTop,
            MessageFormatNotAtTop,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionNotAtTop);

        private static bool IsHookName(string methodName)
        {
            return methodName.Length > 3
                && methodName.StartsWith("Use")
                && char.IsUpper(methodName[3]);
        }

        private static bool IsHookInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax ||
                invocation.Expression is GenericNameSyntax)
            {
                return true;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is ThisExpressionSyntax)
            {
                return true;
            }

            return false;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Rule,
            RuleConditional,
            RuleLoop,
            RuleSwitch,
            RuleNotAtTop);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var methodName = GetMethodName(invocation);
            if (methodName == null || !IsHookName(methodName))
            {
                return;
            }

            if (!IsHookInvocation(invocation))
            {
                return;
            }

            if (!IsValidHookUsage(invocation))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            // Cache results to avoid redundant syntax tree traversals
            var isInConditional = IsInConditionalStatement(invocation);
            var isInLoop = IsInLoop(invocation);
            var isInSwitch = IsInSwitchStatement(invocation);

            if (isInConditional)
            {
                var diagnostic = Diagnostic.Create(RuleConditional, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }

            if (isInLoop)
            {
                var diagnostic = Diagnostic.Create(RuleLoop, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }

            if (isInSwitch)
            {
                var diagnostic = Diagnostic.Create(RuleSwitch, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }

            if (!isInConditional && !isInLoop && !isInSwitch && IsNotAtTopOfMethod(invocation))
            {
                var diagnostic = Diagnostic.Create(RuleNotAtTop, invocation.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static string? GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }

            if (invocation.Expression is GenericNameSyntax genericName)
            {
                return genericName.Identifier.Text;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is IdentifierNameSyntax memberIdentifier)
                {
                    return memberIdentifier.Identifier.Text;
                }

                if (memberAccess.Name is GenericNameSyntax memberGenericName)
                {
                    return memberGenericName.Identifier.Text;
                }
            }

            return null;
        }

        private static bool IsValidHookMethod(string methodName)
        {
            return methodName == "Build" || methodName == "BuildSample";
        }

        private static bool IsValidHookUsage(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                if (current is LambdaExpressionSyntax ||
                    current is LocalFunctionStatementSyntax ||
                    current is AnonymousMethodExpressionSyntax)
                {
                    return false;
                }

                if (current is MethodDeclarationSyntax method)
                {
                    return IsValidHookMethod(method.Identifier.Text);
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsInConditionalStatement(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                if (current is IfStatementSyntax ||
                    current is ConditionalExpressionSyntax ||
                    current is TryStatementSyntax ||
                    current is CatchClauseSyntax ||
                    current is UsingStatementSyntax)
                {
                    return true;
                }

                if (current is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text))
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
                if (current is ForStatementSyntax ||
                    current is ForEachStatementSyntax ||
                    current is WhileStatementSyntax ||
                    current is DoStatementSyntax)
                {
                    return true;
                }

                if (current is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text))
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
                if (current is SwitchStatementSyntax)
                {
                    return true;
                }

                if (current is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text))
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsNotAtTopOfMethod(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;
            MethodDeclarationSyntax? buildMethod = null;

            while (current != null)
            {
                if (current is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text))
                {
                    buildMethod = method;
                    break;
                }
                current = current.Parent;
            }

            if (buildMethod == null)
            {
                return false;
            }

            var body = buildMethod.Body;
            if (body == null)
            {
                return false;
            }

            var statements = body.Statements;
            var invocationSpan = invocation.Span;
            var statementIndex = -1;

            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i].Span.Contains(invocationSpan))
                {
                    statementIndex = i;
                    break;
                }
            }

            if (statementIndex < 0)
            {
                return false;
            }

            for (int i = 0; i < statementIndex; i++)
            {
                var statement = statements[i];

                if (statement is ReturnStatementSyntax)
                {
                    // If a return statement appears before the hook, the hook is unreachable
                    return false;
                }

                if (!IsHookStatement(statement))
                {
                    return true;
                }
            }

            return false;
        }



        private static bool IsHookStatement(StatementSyntax statement)
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
            {
                // Use LINQ for cleaner filtering
                if (localDecl.Declaration.Variables
                    .Where(v => v.Initializer?.Value is InvocationExpressionSyntax)
                    .Any(v =>
                    {
                        var invocation = (InvocationExpressionSyntax)v.Initializer!.Value;
                        var methodName = GetMethodName(invocation);
                        return methodName != null && IsHookName(methodName);
                    }))
                {
                    return true;
                }
            }
            else if (statement is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
            {
                var methodName = GetMethodName(invocation);
                if (methodName != null && IsHookName(methodName))
                {
                    return true;
                }
            }

            return ContainsHookInvocation(statement);
        }

        private static bool ContainsHookInvocation(SyntaxNode node)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is InvocationExpressionSyntax invocation)
                {
                    var methodName = GetMethodName(invocation);
                    if (methodName != null && IsHookName(methodName))
                    {
                        return true;
                    }
                }

                if (current is LambdaExpressionSyntax ||
                    current is LocalFunctionStatementSyntax ||
                    current is AnonymousMethodExpressionSyntax)
                {
                    continue;
                }

                foreach (var child in current.ChildNodes())
                {
                    stack.Push(child);
                }
            }

            return false;
        }
    }
}