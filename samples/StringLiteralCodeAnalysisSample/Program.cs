using StringLiteralGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class Program
{
    static void Main()
    {
        var source = @"using System;
using StringLiteral;

partial class Literals
{
    [StringLiteral.Utf8Attribute(""aαあ😊"")]
    public static partial System.ReadOnlySpan<byte> M1();

    [StringLiteral.Utf8Attribute(""aαあ😊"")]
    protected static partial ReadOnlySpan<byte> M2();

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

    [StringLiteral.HexAttribute(""DEADBEEF"")]
    public static partial System.ReadOnlySpan<byte> H1();

    [StringLiteral.Base64(""AQIDBAUGBwgJCm54goyWoMj/"")]
    public static partial System.ReadOnlySpan<byte> B1();
}

namespace Sample
{
    partial class Literals
    {
        [StringLiteral.Utf8Attribute(""aαあ😊"")]
        public static partial System.ReadOnlySpan<byte> M1();

        [StringLiteral.Utf8Attribute(""aαあ😊"")]
        protected static partial ReadOnlySpan<byte> M2();

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
    }
}";

        var compilation = Compile(source);

        foreach (var diag in compilation.GetDiagnostics())
        {
            Console.WriteLine(diag);
        }
    }

    private static Compilation Compile(string source)
    {
        var opt = new CSharpParseOptions(languageVersion: LanguageVersion.Preview, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        var copt = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var dotnetCoreDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var compilation = CSharpCompilation.Create("test",
            syntaxTrees: new[] { SyntaxFactory.ParseSyntaxTree(source, opt) },
            references: new[]
            {
                    AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference(),
                    MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "System.Runtime.dll")),
            },
            options: copt);

        // apply the source generator
        var driver = CSharpGeneratorDriver.Create(new Utf8StringLiteralGenerator())
            .WithUpdatedParseOptions(opt);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var resultCompilation, out _);

        return resultCompilation;
    }
}
