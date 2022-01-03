﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private static void Emit(SourceProductionContext context, ImmutableArray<Utf8LiteralMethod> methods)
    {
        var buffer = new StringBuilder();

        var group = methods.GroupBy(x => x.Type, x => x.Method);

        foreach (var g in group)
        {
            var containingType = g.Key;
            var generatedSource = Generate(containingType, g, buffer);
            var filename = GetFilename(containingType, buffer);
            context.AddSource(filename, SourceText.From(generatedSource, Encoding.UTF8));
        }
    }

    private static string GetFilename(TypeInfo type, StringBuilder buffer)
    {
        buffer.Clear();

        if (type.Namespace is { } ns)
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }
        buffer.Append(type.Name);
        buffer.Append("_utf8literal.cs");

        return buffer.ToString();
    }

    private static string Generate(TypeInfo type, IEnumerable<MethodInfo> methods, StringBuilder buffer)
    {
        var (ns, name, isValueType) = type;

        buffer.Clear();
        buffer.AppendLine("// <auto-generated />");

        if (ns is not null)
        {
            buffer.Append(@"namespace ");
            buffer.Append(ns);
            buffer.Append(@"
{
");
        }
        buffer.Append(@"partial ");
        if (isValueType)
        {
            buffer.Append("struct ");
        }
        else
        {
            buffer.Append("class ");
        }

        buffer.Append(name);
        buffer.Append(@"
{
");
        foreach (var (methodName, accessibility, value) in methods)
        {
            buffer.Append("    ");
            buffer.Append(AccessibilityText(accessibility));
            buffer.Append(" static partial System.ReadOnlySpan<byte> ");
            buffer.Append(methodName);
            buffer.Append("() => new byte[] {");

            foreach (var b in Encoding.UTF8.GetBytes(value))
            {
                buffer.Append(b);
                buffer.Append(", ");
            }

            buffer.Append(@"};
");
        }

        buffer.Append(@"}
");
        if (ns is not null)
        {
            buffer.Append(@"}
");
        }

        return buffer.ToString();
    }

    private static string AccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new InvalidOperationException(),
    };
}
