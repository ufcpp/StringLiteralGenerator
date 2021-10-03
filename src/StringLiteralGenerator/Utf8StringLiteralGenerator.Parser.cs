using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator : ISourceGenerator
{
    private const string attributeName = "StringLiteral.Utf8Attribute";

    private static bool IsStaticPartial(MemberDeclarationSyntax m)
    {
        bool isStatic = false;
        bool isPartial = false;
        foreach (var mod in m.Modifiers)
        {
            isStatic |= mod.Text == "static";
            isPartial |= mod.Text == "partial";
        }
        return isStatic && isPartial;
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
