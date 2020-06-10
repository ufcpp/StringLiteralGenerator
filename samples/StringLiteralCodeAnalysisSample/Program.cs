using StringLiteralGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.IO;

class Program
{
    static void Main()
    {
        var source = @"using System;
using StringLiteral;

class Program
{
    [StringLiteral.Utf8Attribute(""aαあ😊"")]
    public static partial System.ReadOnlySpan<byte> M1();

    [StringLiteral.Utf8Attribute(""aαあ😊"")]
    protectedstatic partial ReadOnlySpan<byte> M2();

    [Utf8Attribute(""aαあ😊"")]
    private static partial System.ReadOnlySpan<byte> M3();

    [Utf8Attribute(""aαあ😊"")]
    internal static partial ReadOnlySpan<byte> M4();

    [StringLiteral.Utf8(""aαあ😊"")]
    private protected static partial System.ReadOnlySpan<byte> M11();

    [StringLiteral.Utf8(""aαあ😊"")]
    protected internal static partial ReadOnlySpan<byte> M12();

    [Utf8(""aαあ😊"")]
    protected private static partial System.ReadOnlySpan<byte> M13();

    [Utf8(""aαあ😊"")]
    internal protected static partial ReadOnlySpan<byte> M14();

    static void Main() { }
}";

        var compilation = Compile(source);

        foreach (var diag in compilation.GetDiagnostics())
        {
            Console.WriteLine(diag);
        }
    }

    private static Compilation Compile(string source)
    {
        var dotnetCoreDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var compilation = CSharpCompilation.Create("test",
            syntaxTrees: new[] { SyntaxFactory.ParseSyntaxTree(source) },
            references: new[]
            {
                    AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference(),
                    MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "System.Runtime.dll")),
            });

        // ここから
        var opt = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        var driver = new CSharpGeneratorDriver(opt, ImmutableArray.Create<ISourceGenerator>(new Utf8StringLiteralGenerator()), ImmutableArray<AdditionalText>.Empty);
        driver.RunFullGeneration(compilation, out var resultCompilation, out _);
        // ここまで

        return resultCompilation;
    }
}
