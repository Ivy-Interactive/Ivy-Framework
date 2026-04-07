using System.Collections.Generic;
using System.Collections.Immutable;
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
        private const string MessageFormat = "Ivy hook '{0}' must be called at the top level of the Build() method — not inside lambdas, local functions, or helper methods. Hooks must always execute in the same order on every render. See: https://docs.ivy.app/other/ivy/analyser/ivyhook001.";
        private const string Description = "Ivy hooks must be called directly inside the Build() method, not inside lambdas, local functions, or other methods.";

        public const string DiagnosticIdNestedClosure = "IVYHOOK001B";
        private const string TitleNestedClosure = "Ivy Hook Used in Nested Closure";
        private const string MessageFormatNestedClosure = "Ivy hook '{0}' is inside a {1} within Build() — move it to the top level of Build(). Hooks must always execute in the same order on every render. See: https://docs.ivy.app/other/ivy/analyser/ivyhook001b.";
        private const string DescriptionNestedClosure = "Ivy hooks cannot be called inside lambdas, local functions, or anonymous methods even when those are defined within Build(). Move the hook call to the top level of Build().";
        private const string Category = "Usage";

        public const string DiagnosticIdConditional = "IVYHOOK002";
        private const string TitleConditional = "Ivy Hook Called Conditionally";
        private const string MessageFormatConditional = "Ivy hook '{0}' cannot be called conditionally. Hooks must be called in the same order on every render. See: https://docs.ivy.app/other/ivy/analyser/ivyhook002.";
        private const string DescriptionConditional = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside if statements, ternary operators, or other conditional logic.";

        public const string DiagnosticIdLoop = "IVYHOOK003";
        private const string TitleLoop = "Ivy Hook Called in Loop";
        private const string MessageFormatLoop = "Ivy hook '{0}' cannot be called inside a loop. Hooks must be called in the same order on every render. See: https://docs.ivy.app/other/ivy/analyser/ivyhook003.";
        private const string DescriptionLoop = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside for, foreach, while, or do-while loops.";

        public const string DiagnosticIdSwitch = "IVYHOOK004";
        private const string TitleSwitch = "Ivy Hook Called in Switch Statement";
        private const string MessageFormatSwitch = "Ivy hook '{0}' cannot be called inside a switch statement. Hooks must be called in the same order on every render. See: https://docs.ivy.app/other/ivy/analyser/ivyhook004.";
        private const string DescriptionSwitch = "Ivy hooks must be called unconditionally at the top level of the Build() method. Do not call hooks inside switch statements.";

        public const string DiagnosticIdNotAtTop = "IVYHOOK005";
        private const string TitleNotAtTop = "Ivy Hook Not at Top of Build Method";
        private const string MessageFormatNotAtTop = "Ivy hook '{0}' must be called at the top of the Build() method, before any other statements. See: https://docs.ivy.app/other/ivy/analyser/ivyhook005.";
        private const string DescriptionNotAtTop = "All hooks must be called at the very top of the Build() method, before any other non-hook statements. This ensures hooks are called in a consistent order on every render.";

        public const string DiagnosticIdFieldStorage = "IVYHOOK006";
        private const string TitleFieldStorage = "Hook Result Stored in Class Member";
        private const string MessageFormatFieldStorage = "Ivy hook '{0}' result must not be stored in a class field or property. Use a local variable instead. See: https://docs.ivy.app/other/ivy/analyser/ivyhook006.";
        private const string DescriptionFieldStorage = "Storing hook results in class fields or properties breaks the reactive system. The state object is captured once and reused across renders, causing hooks to receive wrong indices.";

        public const string DiagnosticIdInlineExpression = "IVYHOOK007";
        private const string TitleInlineExpression = "Hook Called Inline in Expression";
        private const string MessageFormatInlineExpression = "'{0}' should be assigned to a local variable at the top of the Build method, not called inline in an expression. Extract to: var myVar = {0}; See: https://docs.ivy.app/other/ivy/analyser/ivyhook007.";
        private const string DescriptionInlineExpression = "Hooks must be assigned to a top-level local variable or called as a standalone expression statement. Do not call hooks inline within widget construction expressions, pipe chains, constructor arguments, or return statements.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        private static readonly DiagnosticDescriptor RuleNestedClosure = new DiagnosticDescriptor(
            DiagnosticIdNestedClosure,
            TitleNestedClosure,
            MessageFormatNestedClosure,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescriptionNestedClosure);

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

        private static readonly DiagnosticDescriptor RuleFieldStorage = new DiagnosticDescriptor(
            DiagnosticIdFieldStorage,
            TitleFieldStorage,
            MessageFormatFieldStorage,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DescriptionFieldStorage);

        private static readonly DiagnosticDescriptor RuleInlineExpression = new DiagnosticDescriptor(
            DiagnosticIdInlineExpression,
            TitleInlineExpression,
            MessageFormatInlineExpression,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: DescriptionInlineExpression);

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
            RuleNestedClosure,
            RuleConditional,
            RuleLoop,
            RuleSwitch,
            RuleNotAtTop,
            RuleFieldStorage,
            RuleInlineExpression);

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
                return;

            if (!IsHookInvocation(invocation))
                return;

            var hookUsage = CheckHookUsage(invocation);
            if (hookUsage != HookUsageResult.Valid)
            {
                ReportInvalidPlacement(context, invocation, methodName, hookUsage);
                return;
            }

            if (IsStoredInClassMember(invocation, context))
                context.ReportDiagnostic(Diagnostic.Create(RuleFieldStorage, invocation.GetLocation(), methodName));

            ReportControlFlowViolations(context, invocation, methodName);
        }

        private static void ReportInvalidPlacement(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            string methodName,
            HookUsageResult hookUsage)
        {
            if (hookUsage == HookUsageResult.OutsideBuildMethod)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodName));
                return;
            }

            var closureKind = hookUsage switch
            {
                HookUsageResult.NestedInLambda => "lambda",
                HookUsageResult.NestedInLocalFunction => "local function",
                HookUsageResult.NestedInAnonymousMethod => "anonymous method",
                _ => "closure"
            };
            context.ReportDiagnostic(Diagnostic.Create(RuleNestedClosure, invocation.GetLocation(), methodName, closureKind));
        }

        private static void ReportControlFlowViolations(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            string methodName)
        {
            var isInConditional = IsInConditionalStatement(invocation);
            var isInLoop = IsInLoop(invocation);
            var isInSwitch = IsInSwitchStatement(invocation);

            if (isInConditional)
                context.ReportDiagnostic(Diagnostic.Create(RuleConditional, invocation.GetLocation(), methodName));

            if (isInLoop)
                context.ReportDiagnostic(Diagnostic.Create(RuleLoop, invocation.GetLocation(), methodName));

            if (isInSwitch)
                context.ReportDiagnostic(Diagnostic.Create(RuleSwitch, invocation.GetLocation(), methodName));

            // Only check inline/position when the hook isn't already flagged by control flow rules
            var isInControlFlow = isInConditional || isInLoop || isInSwitch;
            if (isInControlFlow)
                return;

            if (IsInlineExpression(invocation))
                context.ReportDiagnostic(Diagnostic.Create(RuleInlineExpression, invocation.GetLocation(), methodName));

            if (IsNotAtTopOfMethod(invocation))
                context.ReportDiagnostic(Diagnostic.Create(RuleNotAtTop, invocation.GetLocation(), methodName));
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

        private enum HookUsageResult
        {
            Valid,
            OutsideBuildMethod,
            NestedInLambda,
            NestedInLocalFunction,
            NestedInAnonymousMethod,
        }

        private static bool IsValidHookMethod(string methodName)
        {
            return methodName == "Build" || methodName == "BuildSample";
        }

        private static HookUsageResult CheckHookUsage(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                if (current is LambdaExpressionSyntax lambda)
                {
                    // FuncView/MemoizedFuncView lambdas ARE Build methods — hooks are valid inside them
                    if (IsFuncViewBuilderLambda(lambda))
                    {
                        return HookUsageResult.Valid;
                    }
                    return HookUsageResult.NestedInLambda;
                }

                if (current is LocalFunctionStatementSyntax)
                {
                    return HookUsageResult.NestedInLocalFunction;
                }

                if (current is AnonymousMethodExpressionSyntax)
                {
                    return HookUsageResult.NestedInAnonymousMethod;
                }

                if (current is MethodDeclarationSyntax method)
                {
                    return IsValidHookMethod(method.Identifier.Text)
                        ? HookUsageResult.Valid
                        : HookUsageResult.OutsideBuildMethod;
                }

                current = current.Parent;
            }

            return HookUsageResult.OutsideBuildMethod;
        }

        private static bool IsFuncViewBuilderLambda(LambdaExpressionSyntax lambda)
        {
            // Check if lambda is an argument to: new FuncView(...) or new MemoizedFuncView(...)
            if (lambda.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax creation } })
            {
                var typeName = creation.Type switch
                {
                    IdentifierNameSyntax id => id.Identifier.Text,
                    QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
                    _ => null
                };
                return typeName is "FuncView" or "MemoizedFuncView";
            }
            return false;
        }

        private static bool HasAncestorInBuildScope(
            InvocationExpressionSyntax invocation,
            System.Func<SyntaxNode, bool> predicate)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                if (predicate(current))
                    return true;

                if (IsBuildMethodBoundary(current))
                    return false;

                current = current.Parent;
            }

            return false;
        }

        private static bool IsBuildMethodBoundary(SyntaxNode node) =>
            (node is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text)) ||
            (node is LambdaExpressionSyntax lambda && IsFuncViewBuilderLambda(lambda));

        private static bool IsInConditionalStatement(InvocationExpressionSyntax invocation) =>
            HasAncestorInBuildScope(invocation, node =>
                node is IfStatementSyntax or ConditionalExpressionSyntax
                    or TryStatementSyntax or CatchClauseSyntax or UsingStatementSyntax);

        private static bool IsInLoop(InvocationExpressionSyntax invocation) =>
            HasAncestorInBuildScope(invocation, node =>
                node is ForStatementSyntax or ForEachStatementSyntax
                    or WhileStatementSyntax or DoStatementSyntax);

        private static bool IsInSwitchStatement(InvocationExpressionSyntax invocation) =>
            HasAncestorInBuildScope(invocation, node => node is SwitchStatementSyntax);

        private static bool IsNotAtTopOfMethod(InvocationExpressionSyntax invocation)
        {
            var statements = GetBuildMethodStatements(invocation);
            return statements != null && CheckNotAtTop(invocation, statements.Value);
        }

        private static SyntaxList<StatementSyntax>? GetBuildMethodStatements(InvocationExpressionSyntax invocation)
        {
            var current = invocation.Parent;

            while (current != null)
            {
                if (current is MethodDeclarationSyntax method && IsValidHookMethod(method.Identifier.Text))
                    return method.Body?.Statements;

                if (current is LambdaExpressionSyntax lambda && IsFuncViewBuilderLambda(lambda))
                    return (lambda.Body as BlockSyntax)?.Statements;

                current = current.Parent;
            }

            return null;
        }

        private static bool CheckNotAtTop(InvocationExpressionSyntax invocation, SyntaxList<StatementSyntax> statements)
        {
            var invocationSpan = invocation.Span;
            var statementIndex = FindContainingStatementIndex(statements, invocationSpan);

            if (statementIndex < 0)
                return false;

            for (int i = 0; i < statementIndex; i++)
            {
                if (statements[i] is ReturnStatementSyntax)
                    return false;

                if (!IsHookStatement(statements[i]))
                    return true;
            }

            return false;
        }

        private static int FindContainingStatementIndex(SyntaxList<StatementSyntax> statements, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i].Span.Contains(span))
                    return i;
            }
            return -1;
        }



        private static bool IsHookStatement(StatementSyntax statement)
        {
            if (statement is LocalDeclarationStatementSyntax localDecl)
                return HasHookInitializer(localDecl) || ContainsHookInvocation(statement);

            if (statement is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
                return IsHookCall(invocation) || ContainsHookInvocation(statement);

            return ContainsHookInvocation(statement);
        }

        private static bool HasHookInitializer(LocalDeclarationStatementSyntax localDecl)
        {
            foreach (var variable in localDecl.Declaration.Variables)
            {
                if (variable.Initializer == null)
                    continue;

                if (UnwrapSuppression(variable.Initializer.Value) is InvocationExpressionSyntax invocation
                    && IsHookCall(invocation))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsHookCall(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);
            return methodName != null && IsHookName(methodName);
        }

        private static bool IsStoredInClassMember(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            // Walk up to find the assignment expression containing this invocation
            var current = invocation.Parent;

            while (current != null)
            {
                if (current is AssignmentExpressionSyntax assignment &&
                    (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                     assignment.IsKind(SyntaxKind.CoalesceAssignmentExpression)))
                {
                    return IsClassMemberExpression(assignment.Left, context);
                }

                // Stop at statement level
                if (current is StatementSyntax)
                {
                    break;
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsClassMemberExpression(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
        {
            // this._field or this.Property
            if (expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is ThisExpressionSyntax)
            {
                return true;
            }

            // _field or Property (resolve via semantic model)
            if (expression is IdentifierNameSyntax identifier)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(identifier);
                var symbol = symbolInfo.Symbol;

                if (symbol is IFieldSymbol || symbol is IPropertySymbol)
                {
                    return true;
                }
            }

            return false;
        }

        private static ExpressionSyntax UnwrapSuppression(ExpressionSyntax expression)
        {
            // Unwrap null-forgiving operator: UseHook()! → UseHook()
            while (expression is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } postfix)
            {
                expression = postfix.Operand;
            }
            return expression;
        }

        private static bool IsInlineExpression(InvocationExpressionSyntax invocation)
        {
            var current = (SyntaxNode)invocation;

            while (current != null)
            {
                if (current.Parent is LocalDeclarationStatementSyntax localDecl)
                    return !IsDirectInitializer(invocation, localDecl);

                if (current.Parent is ExpressionStatementSyntax)
                    return !IsAllowedExpressionStatement(invocation, current);

                if (current.Parent is StatementSyntax)
                    return true;

                current = current.Parent;
            }

            return false;
        }

        private static bool IsDirectInitializer(
            InvocationExpressionSyntax invocation,
            LocalDeclarationStatementSyntax localDecl)
        {
            foreach (var variable in localDecl.Declaration.Variables)
            {
                if (variable.Initializer == null)
                    continue;

                if (variable.Initializer.Value == invocation)
                    return true;

                if (UnwrapSuppression(variable.Initializer.Value) == invocation)
                    return true;
            }
            return false;
        }

        private static bool IsAllowedExpressionStatement(
            InvocationExpressionSyntax invocation,
            SyntaxNode current)
        {
            // Standalone expression: UseEffect(() => {});
            if (current == invocation)
                return true;

            // Direct RHS of assignment: _field = UseState(0); (caught by IVYHOOK006)
            if (current is AssignmentExpressionSyntax assignment && assignment.Right == invocation)
                return true;

            return false;
        }

        private static bool ContainsHookInvocation(SyntaxNode node)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is InvocationExpressionSyntax invocation && IsHookCall(invocation))
                    return true;

                // Don't descend into closures — they have their own scope
                if (current is LambdaExpressionSyntax
                    or LocalFunctionStatementSyntax
                    or AnonymousMethodExpressionSyntax)
                {
                    continue;
                }

                foreach (var child in current.ChildNodes())
                    stack.Push(child);
            }

            return false;
        }
    }
}