using Microsoft.CodeAnalysis;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private record BinaryLiteralMethod(TypeInfo Type, MethodInfo Method)
    {
        public BinaryLiteralMethod(IMethodSymbol m, string text, LiteralFormat format)
            : this(new TypeInfo(m.ContainingType), new MethodInfo(m, text, format))
        { }
    }

    private enum LiteralFormat
    {
        Utf8 = 1,
        Hex,
        Base64,
    }

    private record struct TypeInfo(string? Namespace, string Name, bool IsValueType)
    {
        public TypeInfo(INamedTypeSymbol t)
            : this(GetNamespace(t), t.Name, t.IsValueType)
        { }

        private static string? GetNamespace(INamedTypeSymbol t)
        {
            var x = t.ContainingNamespace;
            return string.IsNullOrEmpty(x.Name) ? null : x.ToDisplayString();
        }
    }

    private record struct MethodInfo(string Name, Accessibility Accessibility, string Text, LiteralFormat Format)
    {
        public MethodInfo(IMethodSymbol m, string text, LiteralFormat format)
            : this(m.Name, m.DeclaredAccessibility, text, format)
        { }
    }
}

