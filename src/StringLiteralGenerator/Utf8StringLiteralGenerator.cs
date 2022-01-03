namespace StringLiteralGenerator;

[Generator(LanguageNames.CSharp)]
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

    private static void AddAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("Utf8Attribute", SourceText.From(attributeText, Encoding.UTF8));
    }

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
}
