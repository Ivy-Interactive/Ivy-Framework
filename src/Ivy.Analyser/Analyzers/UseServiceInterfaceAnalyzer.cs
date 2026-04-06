using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseServiceInterfaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IVYSERVICE001";
        private const string Title = "UseService should use interface type";
        private const string MessageFormat = "UseService<{0}> should use interface I{0} instead. Using concrete types breaks testability and violates dependency inversion.";
        private const string Description = "When calling UseService<T>(), prefer interface types over concrete types when an interface is available in the same namespace.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

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

            if (!IsUseServiceCall(invocation, out var genericName) || genericName == null)
                return;

            if (genericName.TypeArgumentList.Arguments.Count != 1)
                return;

            var typeArgument = genericName.TypeArgumentList.Arguments[0];
            var typeSymbol = context.SemanticModel.GetTypeInfo(typeArgument).Type;

            if (typeSymbol == null || typeSymbol.TypeKind == TypeKind.Interface)
                return;

            var interfaceName = "I" + typeSymbol.Name;
            var interfaceSymbol = FindInterfaceInNamespace(typeSymbol, interfaceName);

            if (interfaceSymbol != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    typeArgument.GetLocation(),
                    typeSymbol.Name));
            }
        }

        private static bool IsUseServiceCall(InvocationExpressionSyntax invocation, out GenericNameSyntax? genericName)
        {
            genericName = null;

            if (invocation.Expression is GenericNameSyntax gns &&
                gns.Identifier.Text == "UseService")
            {
                genericName = gns;
                return true;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax gns2 &&
                gns2.Identifier.Text == "UseService")
            {
                genericName = gns2;
                return true;
            }

            return false;
        }

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
