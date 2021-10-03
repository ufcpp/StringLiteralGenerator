using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace StringLiteralGenerator;

[Generator]
public partial class Utf8StringLiteralGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

        var compilation = context.Compilation;

        Emit(context, enumerate());

        IEnumerable<Utf8LiteralMethod> enumerate()
        {
            foreach (var m in receiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(m.SyntaxTree);

                if (GetSemanticTargetForGeneration(model, m) is { } t)
                    yield return t;
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(AddAttribute);
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for property generation
            if (IsSyntaxTargetForGeneration(syntaxNode))
            {
                CandidateMethods.Add((MethodDeclarationSyntax)syntaxNode);
            }
        }
    }
}
