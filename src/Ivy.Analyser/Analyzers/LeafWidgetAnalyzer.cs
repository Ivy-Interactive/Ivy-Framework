using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LeafWidgetAnalyzer : DiagnosticAnalyzer
    {
        public const string LeafDiagnosticId = "IVYCHILD001";
        public const string SingleChildDiagnosticId = "IVYCHILD002";
        public const string WrongChildTypeDiagnosticId = "IVYCHILD003";

        private static readonly DiagnosticDescriptor LeafRule = new DiagnosticDescriptor(
            LeafDiagnosticId,
            "Adding Children to Leaf Widget",
            "'{0}' does not support children. See: https://docs.ivy.app/other/ivy/analyser/ivychild001.",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "This widget does not support children. Adding children via the | operator will throw NotSupportedException at runtime.");

        private static readonly DiagnosticDescriptor SingleChildRule = new DiagnosticDescriptor(
            SingleChildDiagnosticId,
            "Adding Multiple Children to Single-Child Widget",
            "'{0}' only supports a single child. See: https://docs.ivy.app/other/ivy/analyser/ivychild002.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "This widget only supports a single child. Adding multiple children via chained | operators will throw NotSupportedException at runtime.");

        private static readonly DiagnosticDescriptor WrongChildTypeRule = new DiagnosticDescriptor(
            WrongChildTypeDiagnosticId,
            "Wrong Child Type for Widget",
            "'{0}' only accepts children of type '{1}'. Got '{2}'. See: https://docs.ivy.app/other/ivy/analyser/ivychild003.",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "This widget only accepts children of a specific type. Passing a different type via the | operator will throw NotSupportedException at runtime.");

        private static readonly HashSet<string> LeafWidgetTypes =
        [
            "Ivy.Button",
            "Ivy.Badge",
            "Ivy.Progress",
            "Ivy.Field",
            "Ivy.Detail",
            "Ivy.Dialog",
            "Ivy.DialogHeader",
            "Ivy.HeaderLayout",
            "Ivy.SidebarLayout",
            "Ivy.SidebarMenu",
            "Ivy.FooterLayout",
            "Ivy.DataTable",
            "Ivy.LineChart",
            "Ivy.PieChart",
            "Ivy.BarChart",
            "Ivy.AreaChart",
            "Ivy.Tooltip"
        ];

        private static readonly HashSet<string> LeafInterfaceTypes = new()
        {
            "Ivy.IInput",
        };

        private static readonly HashSet<string> SingleChildWidgetTypes = new()
        {
            "Ivy.Card",
            "Ivy.Sheet",
            "Ivy.Confetti",
            "Ivy.FloatingPanel",
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(LeafRule, SingleChildRule, WrongChildTypeRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBitwiseOr, SyntaxKind.BitwiseOrExpression);
        }

        private static void AnalyzeBitwiseOr(SyntaxNodeAnalysisContext context)
        {
            var binaryExpr = (BinaryExpressionSyntax)context.Node;
            var leftType = context.SemanticModel.GetTypeInfo(binaryExpr.Left, context.CancellationToken).Type;

            if (leftType == null)
                return;

            if (IsLeafWidget(leftType))
            {
                var diagnostic = Diagnostic.Create(LeafRule, binaryExpr.GetLocation(), leftType.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            // Check for type-restricted children via [ChildType] attribute
            var widgetType = leftType;
            if (binaryExpr.Left is BinaryExpressionSyntax leftChained
                && leftChained.IsKind(SyntaxKind.BitwiseOrExpression))
            {
                widgetType = GetRootType(leftChained, context.SemanticModel, context.CancellationToken) ?? leftType;
            }

            var allowedChildType = GetChildTypeFromAttribute(widgetType);
            if (allowedChildType != null)
            {
                var rightType = context.SemanticModel.GetTypeInfo(binaryExpr.Right, context.CancellationToken).Type;
                if (rightType != null && !IsCompatibleChildType(rightType, allowedChildType))
                {
                    var diagnostic = Diagnostic.Create(WrongChildTypeRule, binaryExpr.GetLocation(),
                        widgetType.Name, allowedChildType.Name, rightType.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            if (binaryExpr.Left is BinaryExpressionSyntax leftBinary
                && leftBinary.IsKind(SyntaxKind.BitwiseOrExpression))
            {
                var rootType = GetRootType(leftBinary, context.SemanticModel, context.CancellationToken);
                if (rootType != null && IsSingleChildWidget(rootType))
                {
                    var diagnostic = Diagnostic.Create(SingleChildRule, binaryExpr.GetLocation(), rootType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static ITypeSymbol? GetRootType(BinaryExpressionSyntax expr, SemanticModel model,
            System.Threading.CancellationToken ct)
        {
            var current = expr;
            while (current.Left is BinaryExpressionSyntax left && left.IsKind(SyntaxKind.BitwiseOrExpression))
            {
                current = left;
            }

            return model.GetTypeInfo(current.Left, ct).Type;
        }

        private static bool IsLeafWidget(ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                if (LeafWidgetTypes.Contains(GetFullTypeName(current)))
                    return true;
                current = current.BaseType;
            }

            foreach (var iface in type.AllInterfaces)
            {
                if (LeafInterfaceTypes.Contains(GetFullTypeName(iface.OriginalDefinition)))
                    return true;
            }

            return false;
        }

        private static bool IsSingleChildWidget(ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                if (SingleChildWidgetTypes.Contains(GetFullTypeName(current)))
                    return true;
                current = current.BaseType;
            }

            return false;
        }

        private static ITypeSymbol? GetChildTypeFromAttribute(ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                foreach (var attr in current.GetAttributes())
                {
                    if (attr.AttributeClass is { Name: "ChildTypeAttribute" })
                    {
                        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is ITypeSymbol allowedType)
                        {
                            return allowedType;
                        }
                    }
                }

                current = current.BaseType;
            }

            return null;
        }

        private static bool IsCompatibleChildType(ITypeSymbol actualType, ITypeSymbol allowedType)
        {
            // Direct match or subtype
            if (IsOrDerivedFrom(actualType, allowedType))
                return true;

            // Check if it's an array or IEnumerable<T> where T is compatible
            if (actualType is IArrayTypeSymbol arrayType)
                return IsOrDerivedFrom(arrayType.ElementType, allowedType);

            foreach (var iface in actualType.AllInterfaces)
            {
                if (iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                    && iface.TypeArguments.Length == 1
                    && IsOrDerivedFrom(iface.TypeArguments[0], allowedType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsOrDerivedFrom(ITypeSymbol type, ITypeSymbol target)
        {
            var current = type;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, target))
                    return true;
                current = current.BaseType;
            }

            return false;
        }

        private static string GetFullTypeName(ITypeSymbol type)
        {
            var ns = type.ContainingNamespace;
            if (ns == null || ns.IsGlobalNamespace)
                return type.Name;
            return ns.ToDisplayString() + "." + type.Name;
        }
    }
}
