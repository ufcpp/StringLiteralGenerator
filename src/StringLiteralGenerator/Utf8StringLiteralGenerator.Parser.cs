using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private const string attributeName = "StringLiteral.Utf8Attribute";

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static Utf8LiteralMethod? GetSemanticTargetForGeneration(SemanticModel semanticModel, MethodDeclarationSyntax m)
    {
        if (m.ParameterList.Parameters.Count != 0) return null;
        if (semanticModel.GetDeclaredSymbol(m) is not { } methodSymbol) return null;
        if (!methodSymbol.IsPartialDefinition || !methodSymbol.IsStatic) return null;
        if (!ReturnsString(methodSymbol)) return null;
        if (GetUtf8Attribute(methodSymbol) is not { } value) return null;

        return new(methodSymbol, value);
    }

    static bool ReturnsString(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnType is INamedTypeSymbol s
            && s.ToDisplayString() == "System.ReadOnlySpan<byte>";
    }

    static string? GetUtf8Attribute(IMethodSymbol methodSymbol)
    {
        foreach (var a in methodSymbol.GetAttributes())
        {
            if (a.AttributeClass?.ToDisplayString() == attributeName)
            {
                var args = a.ConstructorArguments;
                if (args.Length != 1) continue;

                if (args[0].Value is string value) return value;
            }
        }

        return null;
    }
}
