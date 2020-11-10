# C# StringLiteralGenerator

A C# [Source Generator](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) for optimizing UTF-8 binaries.

Original source (manually written):

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

## NuGet

[![NuGet](https://img.shields.io/nuget/v/StringLiteralGenerator?style=flat-square)](https://www.nuget.org/packages/StringLiteralGenerator)

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="StringLiteralGenerator" Version="1.0.0" />
    </ItemGroup>

</Project>
```

For versions earlier than .NET 5 SDK RC2 you may also need to add a reference to `Microsoft.Net.Compilers.Toolset`.
So the `csproj` may look like this:
```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="StringLiteralGenerator" Version="1.0.0-preiew" />
        <PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="3.8.0-4.final" PrivateAssets="all" />
    </ItemGroup>

</Project>
```
