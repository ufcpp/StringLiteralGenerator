using System;
using System.Text;
using StringLiteral;

namespace A
{
    partial class Literals
    {
        [Utf8("ABCDEFGHIJKLMN")]
        public static partial ReadOnlySpan<byte> M1();

        [StringLiteral.Utf8("aαあ亜😊")]
        public static partial System.ReadOnlySpan<byte> M2();
    }
}

partial class Program
{
    [Utf8("ABCDEFGHIJKLMN")]
    private static partial ReadOnlySpan<byte> M1();

    [StringLiteral.Utf8("aαあ亜😊")]
    protected static partial System.ReadOnlySpan<byte> M2();

    static void Main()
    {
        write(M1());
        write(M2());
        write(A.Literals.M1());
        write(A.Literals.M2());

        static void write(ReadOnlySpan<byte> utf8)
        {
            foreach (var b in utf8)
            {
                Console.Write($"{b:X}, ");
            }
            Console.WriteLine(Encoding.UTF8.GetString(utf8));
        }
    }
}
