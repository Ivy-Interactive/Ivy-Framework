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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AppConstructorCodeFixProvider)), Shared]
    public class AppConstructorCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add parameterless constructor";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(AppConstructorAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddParameterlessConstructorAsync(context.Document, classDecl, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddParameterlessConstructorAsync(
            Document document,
            ClassDeclarationSyntax classDecl,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var className = classDecl.Identifier.Text;
            var eol = DetectLineEnding(classDecl);
            var members = classDecl.Members;

            // Find insertion point: before the first existing constructor
            int insertIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] is ConstructorDeclarationSyntax)
                {
                    insertIndex = i;
                    break;
                }
                else if (members[i] is FieldDeclarationSyntax || members[i] is PropertyDeclarationSyntax)
                {
                    insertIndex = i + 1;
                }
            }

            // Determine indent from existing members
            var indent = "        ";
            if (members.Count > 0)
            {
                var refMember = insertIndex < members.Count ? members[insertIndex] : members[members.Count - 1];
                indent = GetIndentation(refMember.GetLeadingTrivia());
            }

            // Parse constructor from string to guarantee consistent line endings
            var ctorSource = $"public {className}(){eol}{indent}{{{eol}{indent}}}";
            var parsed = SyntaxFactory.ParseMemberDeclaration(ctorSource);
            if (parsed is not ConstructorDeclarationSyntax constructor)
                return document;

            // Get the member we're inserting before
            var followingMember = insertIndex < members.Count ? members[insertIndex] : null;

            if (followingMember != null)
            {
                // Take the leading trivia from the following member for the new constructor
                constructor = constructor.WithLeadingTrivia(followingMember.GetLeadingTrivia());

                // Give the following member a blank-line-separated leading trivia
                var newFollowingTrivia = SyntaxFactory.TriviaList(
                    SyntaxFactory.EndOfLine(eol),
                    SyntaxFactory.EndOfLine(eol),
                    SyntaxFactory.Whitespace(indent));
                var updatedFollowing = followingMember.WithLeadingTrivia(newFollowingTrivia);

                var newMembers = members
                    .Replace(followingMember, updatedFollowing)
                    .Insert(insertIndex, constructor);

                var newClassDecl = classDecl.WithMembers(newMembers);
                return document.WithSyntaxRoot(root.ReplaceNode(classDecl, newClassDecl));
            }
            else
            {
                constructor = constructor.WithLeadingTrivia(
                    SyntaxFactory.EndOfLine(eol),
                    SyntaxFactory.Whitespace(indent));

                var newClassDecl = classDecl.WithMembers(members.Insert(insertIndex, constructor));
                return document.WithSyntaxRoot(root.ReplaceNode(classDecl, newClassDecl));
            }
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

        private static string GetIndentation(SyntaxTriviaList trivia)
        {
            for (int i = trivia.Count - 1; i >= 0; i--)
            {
                if (trivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
                    return trivia[i].ToString();
            }

            return "        ";
        }
    }
}
