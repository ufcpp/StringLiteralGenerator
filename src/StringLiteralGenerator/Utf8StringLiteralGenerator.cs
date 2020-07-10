using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace StringLiteralGenerator
{
    [Generator]
    public class Utf8StringLiteralGenerator : ISourceGenerator
    {
        private const string attributeText = @"using System;
namespace StringLiteral
{
    [System.Diagnostics.Conditional(""COMPILE_TIME_ONLY"")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class Utf8Attribute : Attribute
    {
        public Utf8Attribute(string s) { }
    }
}
";
        public void Execute(SourceGeneratorContext context)
        {
            context.AddSource("Utf8Attribute", SourceText.From(attributeText, Encoding.UTF8));

            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            CSharpParseOptions options = (CSharpParseOptions)((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options;

            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

            if (!(compilation.GetTypeByMetadataName("StringLiteral.Utf8Attribute") is { } attributeSymbol)) return;
            if (!(compilation.GetTypeByMetadataName("System.ReadOnlySpan`1") is { } spanSymbol)) return;
            if (!(compilation.GetTypeByMetadataName("System.Byte") is { } byteSymbol)) return;

            var byteSpanSymbol = spanSymbol.Construct(byteSymbol);

            var buffer = new StringBuilder();

            foreach (var m in receiver.CandidateMethods)
            {
                if (!isStaticPartial(m)) continue;

                SemanticModel model = compilation.GetSemanticModel(m.SyntaxTree);

                if (m.ParameterList.Parameters.Count != 0) continue;
                if (!returnsString(model, m)) continue;
                if (!(getUtf8Attribute(model, m) is ({ } value, var containingType, var accessibility))) continue;

                var methodName = m.Identifier.ValueText;
                var classSource = generate(containingType, methodName, value, accessibility);

                var filename = $"{containingType.Name}_{methodName}_utf8literal.cs";
                if (!string.IsNullOrEmpty(containingType.ContainingNamespace.Name))
                {
                    filename = containingType.ContainingNamespace.Name.Replace('.', '/') + filename;
                }
                context.AddSource(filename, SourceText.From(classSource, Encoding.UTF8));
            }

            //todo: group by typeSymbol?
            string generate(INamedTypeSymbol containingType, string methodName, string value, Accessibility accessibility)
            {
                buffer.Clear();

                if (!string.IsNullOrEmpty(containingType.ContainingNamespace.Name))
                {
                    buffer.Append(@"namespace ");
                    buffer.Append(containingType.ContainingNamespace.ToDisplayString());
                    buffer.Append(@"
{
");
                }
                buffer.Append(@"partial class ");
                buffer.Append(containingType.Name);
                buffer.Append(@"
{
    ");
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
}
");
                if (!string.IsNullOrEmpty(containingType.ContainingNamespace.Name))
                {
                    buffer.Append(@"}
");
                }

                return buffer.ToString();
            }

            static bool isStaticPartial(MemberDeclarationSyntax m)
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

            bool returnsString(SemanticModel model, MethodDeclarationSyntax m)
            {
                return model.GetSymbolInfo(m.ReturnType).Symbol is INamedTypeSymbol s
                    && SymbolEqualityComparer.Default.Equals(s, byteSpanSymbol);
            }

            (string? value, INamedTypeSymbol containingType, Accessibility accessibility) getUtf8Attribute(SemanticModel model, MethodDeclarationSyntax m)
            {
                if (!(model.GetDeclaredSymbol(m) is { } s)) return default;

                foreach (var a in s.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol))
                    {
                        var args = a.ConstructorArguments;
                        if (args.Length != 1) continue;

                        if (args[0].Value is string value) return (value, s.ContainingType, s.DeclaredAccessibility);
                    }
                }

                return default;
            }
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

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any field with at least one attribute is a candidate for property generation
                if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
                    && methodDeclarationSyntax.AttributeLists.Count > 0)
                {
                    CandidateMethods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}
