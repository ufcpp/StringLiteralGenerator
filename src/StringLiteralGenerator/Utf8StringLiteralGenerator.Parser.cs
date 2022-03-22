using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private const string utf8AttributeName = "StringLiteral.Utf8Attribute";
    private const string hexAttributeName = "StringLiteral.HexAttribute";
    private const string base64AttributeName = "StringLiteral.Base64Attribute";

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static BinaryLiteralMethod? GetSemanticTargetForGeneration(SemanticModel semanticModel, MethodDeclarationSyntax m)
    {
        if (m.ParameterList.Parameters.Count != 0) return null;
        if (semanticModel.GetDeclaredSymbol(m) is not { } methodSymbol) return null;
        if (!methodSymbol.IsPartialDefinition || !methodSymbol.IsStatic) return null;
        if (!ReturnsString(methodSymbol)) return null;
        if (GetUtf8Attribute(methodSymbol) is not ({ } value, var format)) return null;

        return new(methodSymbol, value, format);
    }

    static bool ReturnsString(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnType is INamedTypeSymbol s
            && s.ToDisplayString() == "System.ReadOnlySpan<byte>";
    }

    static (string?, LiteralFormat) GetUtf8Attribute(IMethodSymbol methodSymbol)
    {
        foreach (var a in methodSymbol.GetAttributes())
        {
            var format = a.AttributeClass?.ToDisplayString() switch
            {
                utf8AttributeName => LiteralFormat.Utf8,
                hexAttributeName => LiteralFormat.Hex,
                base64AttributeName => LiteralFormat.Base64,
                _ => (LiteralFormat)0
            };

            if (format != 0)
            {
                var args = a.ConstructorArguments;
                if (args.Length != 1) continue;

                if (args[0].Value is string value) return (value, format);
            }
        }

        return default;
    }
}
