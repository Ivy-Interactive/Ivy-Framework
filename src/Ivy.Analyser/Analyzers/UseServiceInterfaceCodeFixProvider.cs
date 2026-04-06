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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseServiceInterfaceCodeFixProvider)), Shared]
    public class UseServiceInterfaceCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(UseServiceInterfaceAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type argument node that triggered the diagnostic
            var typeArgument = root.FindNode(diagnosticSpan) as TypeSyntax;
            if (typeArgument == null)
                return;

            // Get semantic model to verify interface exists
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return;

            var typeInfo = semanticModel.GetTypeInfo(typeArgument, context.CancellationToken);
            var typeSymbol = typeInfo.Type;
            if (typeSymbol == null)
                return;

            // Construct interface name by prefixing with 'I'
            var interfaceName = "I" + typeSymbol.Name;

            // Verify the interface exists using the same logic as the analyzer
            var interfaceSymbol = FindInterfaceInNamespace(typeSymbol, interfaceName);
            if (interfaceSymbol == null)
                return; // Don't offer fix if interface doesn't exist

            // Register the code fix
            var title = $"Use interface {interfaceName}";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithInterfaceAsync(context.Document, typeArgument, interfaceName, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Document> ReplaceWithInterfaceAsync(
            Document document,
            TypeSyntax typeArgument,
            string interfaceName,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            // Create new identifier with interface name
            var newTypeArgument = SyntaxFactory.IdentifierName(interfaceName)
                .WithTriviaFrom(typeArgument);

            // Replace the type argument in the syntax tree
            var newRoot = root.ReplaceNode(typeArgument, newTypeArgument);

            return document.WithSyntaxRoot(newRoot);
        }

        // Reuse the same interface lookup logic as UseServiceInterfaceAnalyzer
        private static INamedTypeSymbol? FindInterfaceInNamespace(
            ITypeSymbol typeSymbol,
            string interfaceName)
        {
            var containingNamespace = typeSymbol.ContainingNamespace;
            if (containingNamespace == null)
                return null;

            foreach (var member in containingNamespace.GetMembers(interfaceName))
            {
                if (member is INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceSymbol &&
                    interfaceSymbol.DeclaredAccessibility == Accessibility.Public)
                {
                    return interfaceSymbol;
                }
            }

            return null;
        }
    }
}
