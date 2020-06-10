# C# StringLiteralGenerator

A C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) for optimizing UTF-8 binaries.

Original source (mamual written):

```cs
namespace Sample
{
    partial class Literals
    {
        [StringLiteral.Utf8Attribute("aαあ😊")]
        public static partial System.ReadOnlySpan<byte> S();
    }
}
```

Generated source:

```cs
namespace Sample
{
    partial class Literals
    {
        public static partial System.ReadOnlySpan<byte> S() => new byte[] {97, 206, 177, 227, 129, 130, 240, 159, 152, 138, };
    }
}
```

- Generates UTF-8 binary data from string literals (UTF-16).
- The UTF-8 data is optimized to avoid allocation. see: [C# ReadOnlySpan and static data](https://vcsjones.dev/2019/02/01/csharp-readonly-span-bytes-static/)
