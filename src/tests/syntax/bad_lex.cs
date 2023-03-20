public sealed partial class SyntaxTests
{
    [Fact]
    public Task bad_lex()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                $ !
                0b 0B
                0o 0O
                0x 0X
                1. 1.0e 1.0e+ 1.0e-
                "unclosed string literal
                "bad unicode sequences \u \u1 \u12 \u123 \u1234 \u12345 \uffffff"
                "bad escape sequence \x"
            }
            """);
    }
}
