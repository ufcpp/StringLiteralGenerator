namespace StringLiteralGenerator;

[Generator(LanguageNames.CSharp)]
public partial class Utf8StringLiteralGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not Receiver receiver)
        {
            return;
        }

        var buffer = new StringBuilder();
        var group = receiver.Methods.GroupBy(x => x.Type, x => x.Method);

        foreach (var g in group)
        {
            var containingType = g.Key;
            var generatedSource = Generate(containingType, g, buffer);
            var filename = GetFilename(containingType, buffer);
            context.AddSource(filename, SourceText.From(generatedSource, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(AddAttribute);
        context.RegisterForSyntaxNotifications(() => new Receiver());
    }

    private static void AddAttribute(GeneratorPostInitializationContext context)
    {
        context.AddSource("Utf8Attribute", SourceText.From(attributeText, Encoding.UTF8));
    }
}
