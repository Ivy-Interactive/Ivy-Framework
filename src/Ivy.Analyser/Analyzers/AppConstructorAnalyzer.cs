using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AppConstructorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "IVYAPP001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "App class must have a parameterless constructor",
        "App '{0}' must have a parameterless constructor, use UseService<T>() inside Build() instead of constructor injection. See: https://docs.ivy.app/other/ivy/analyser/ivyapp001.",
        "Ivy.Apps",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "App classes are instantiated via Activator.CreateInstance and require a parameterless constructor. Use UseService<T>() inside Build() for dependency injection.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (namedType.TypeKind != TypeKind.Class || namedType.IsAbstract)
            return;

        // Check for [App] attribute
        var hasAppAttribute = false;
        foreach (var attr in namedType.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "AppAttribute" &&
                attr.AttributeClass.ContainingNamespace?.ToString() == "Ivy")
            {
                hasAppAttribute = true;
                break;
            }
        }

        if (!hasAppAttribute)
            return;

        // Check if any constructor is callable without arguments
        var hasParameterlessCallable = false;
        foreach (var ctor in namedType.Constructors)
        {
            if (ctor.IsStatic)
                continue;

            if (ctor.IsImplicitlyDeclared)
            {
                hasParameterlessCallable = true;
                break;
            }

            if (ctor.Parameters.All(p => p.HasExplicitDefaultValue || p.IsOptional))
            {
                hasParameterlessCallable = true;
                break;
            }
        }

        if (!hasParameterlessCallable)
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
