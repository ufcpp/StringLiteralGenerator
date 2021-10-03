using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StringLiteralGenerator;

[Generator]
public partial class Utf8StringLiteralGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddAttribute);

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsSyntaxTargetForGeneration(node),
                static (context, _) => GetSemanticTargetForGeneration(context.SemanticModel, (MethodDeclarationSyntax)context.Node)!
                )
            .Where(x => x is not null)
            .Collect();

        context.RegisterImplementationSourceOutput(provider, Emit);
    }
}
