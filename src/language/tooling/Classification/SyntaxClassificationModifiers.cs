namespace Vezel.Celerity.Language.Tooling.Classification;

[Flags]
public enum SyntaxClassificationModifiers
{
    None = 0b0000000,
    Public = 0b0000001,
    Opaque = 0b0000010,
    External = 0b0000100,
    Fallible = 0b0001000,
    Mutable = 0b0010000,
    Discard = 0b0100000,
    Upvalue = 0b1000000,
}
