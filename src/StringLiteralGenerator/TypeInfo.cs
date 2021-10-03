﻿using Microsoft.CodeAnalysis;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private record struct TypeInfo(string? Namespace, string Name)
    {
        public TypeInfo(INamedTypeSymbol t)
            : this(GetNamespace(t), t.Name)
        { }

        private static string? GetNamespace(INamedTypeSymbol t)
        {
            var x = t.ContainingNamespace;
            return string.IsNullOrEmpty(x.Name) ? null : x.ToDisplayString();
        }
    }

    private record struct MethodInfo(string Name, Accessibility Accessibility, string Text)
    {
        public MethodInfo(IMethodSymbol m, string text)
            : this(m.Name, m.DeclaredAccessibility, text)
        { }
    }
}