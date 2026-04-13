using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ivy.Analyser.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HookUsageInlineCodeFixProvider)), Shared]
    public class HookUsageInlineCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Extract hook to variable";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(HookUsageAnalyzer.DiagnosticIdInlineExpression);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the hook invocation at the diagnostic span
            // Walk descendants to find the exact hook call (not an outer invocation like CreateWidget(...))
            var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
            InvocationExpressionSyntax? invocation = null;

            if (node is InvocationExpressionSyntax inv && IsHookCall(inv))
            {
                invocation = inv;
            }
            else
            {
                // Search descendants for the hook invocation within the diagnostic span
                foreach (var descendant in node.DescendantNodesAndSelf())
                {
                    if (descendant is InvocationExpressionSyntax candidate &&
                        candidate.Span == diagnosticSpan &&
                        IsHookCall(candidate))
                    {
                        invocation = candidate;
                        break;
                    }
                }

                // Fallback: walk up from node to find the nearest hook invocation
                if (invocation == null)
                {
                    var current = node;
                    while (current != null)
                    {
                        if (current is InvocationExpressionSyntax ancestor && IsHookCall(ancestor))
                        {
                            invocation = ancestor;
                            break;
                        }

                        current = current.Parent;
                    }
                }
            }

            if (invocation == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ExtractHookToVariableAsync(context.Document, invocation, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> ExtractHookToVariableAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var containingBlock = FindContainingBuildBlock(invocation);
            if (containingBlock == null)
                return document;

            var hookName = GetHookMethodName(invocation);
            var baseName = GenerateVariableName(hookName ?? "hook");
            var variableName = EnsureUniqueName(baseName, containingBlock);

            // Find the statement that contains this invocation
            var containingStatement = invocation.Ancestors().OfType<StatementSyntax>().First();

            // Create: var {name} = {hookCall};
            var variableDeclaration = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(variableName))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                invocation.WithoutTrivia())))))
                .WithLeadingTrivia(containingStatement.GetLeadingTrivia())
                .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(DetectLineEnding(containingBlock))));

            // Replace the inline hook call with the variable reference
            var identifierName = SyntaxFactory.IdentifierName(variableName);
            var newContainingStatement = containingStatement.ReplaceNode(invocation, identifierName);

            // Build new statement list: insert declaration at the TOP of Build(), replace the original
            var statements = containingBlock.Statements;
            var newStatements = new List<StatementSyntax>();

            // Insert the declaration at position 0 (top of method)
            newStatements.Add(variableDeclaration);

            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i] == containingStatement)
                {
                    newStatements.Add(newContainingStatement);
                }
                else
                {
                    newStatements.Add(statements[i]);
                }
            }

            var newBlock = containingBlock.WithStatements(SyntaxFactory.List(newStatements));
            var newRoot = root.ReplaceNode(containingBlock, newBlock);
            return document.WithSyntaxRoot(newRoot);
        }

        private static BlockSyntax? FindContainingBuildBlock(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is MethodDeclarationSyntax method &&
                    (method.Identifier.Text == "Build" || method.Identifier.Text == "BuildSample"))
                {
                    return method.Body;
                }

                current = current.Parent;
            }

            return null;
        }

        private static bool IsHookCall(InvocationExpressionSyntax invocation)
        {
            var name = GetHookMethodName(invocation);
            return name != null && name.Length > 3 && name.StartsWith("Use") && char.IsUpper(name[3]);
        }

        private static string? GetHookMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;

            if (invocation.Expression is GenericNameSyntax genericName)
                return genericName.Identifier.Text;

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is IdentifierNameSyntax memberIdentifier)
                    return memberIdentifier.Identifier.Text;

                if (memberAccess.Name is GenericNameSyntax memberGenericName)
                    return memberGenericName.Identifier.Text;
            }

            return null;
        }

        private static string GenerateVariableName(string hookName)
        {
            if (hookName.Length > 3 && hookName.StartsWith("Use") && char.IsUpper(hookName[3]))
            {
                var name = hookName.Substring(3);
                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }

            return "hook";
        }

        private static string EnsureUniqueName(string baseName, BlockSyntax block)
        {
            var existingNames = new HashSet<string>();

            foreach (var statement in block.Statements)
            {
                if (statement is LocalDeclarationStatementSyntax localDecl)
                {
                    foreach (var variable in localDecl.Declaration.Variables)
                    {
                        existingNames.Add(variable.Identifier.Text);
                    }
                }
            }

            if (block.Parent is MethodDeclarationSyntax method)
            {
                foreach (var param in method.ParameterList.Parameters)
                {
                    existingNames.Add(param.Identifier.Text);
                }
            }

            if (!existingNames.Contains(baseName))
                return baseName;

            for (int i = 2; i < 100; i++)
            {
                var candidate = baseName + i;
                if (!existingNames.Contains(candidate))
                    return candidate;
            }

            return baseName;
        }

        private static string DetectLineEnding(SyntaxNode node)
        {
            foreach (var trivia in node.DescendantTrivia())
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    return trivia.ToString();
            }

            return "\n";
        }
    }
}
