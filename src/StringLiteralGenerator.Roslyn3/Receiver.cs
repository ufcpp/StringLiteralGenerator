namespace StringLiteralGenerator;

public partial class Utf8StringLiteralGenerator
{
    private sealed class Receiver : ISyntaxContextReceiver
    {
        public readonly List<Utf8LiteralMethod> Methods = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!IsSyntaxTargetForGeneration(context.Node))
            {
                return;
            }

            var value = GetSemanticTargetForGeneration(context.SemanticModel, (MethodDeclarationSyntax)context.Node);
            if (value is null)
            {
                return;
            }

            Methods.Add(value);
        }
    }
}
