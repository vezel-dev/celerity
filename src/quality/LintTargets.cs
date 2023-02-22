namespace Vezel.Celerity.Quality;

[Flags]
public enum LintTargets
{
    None = 0b00000,
    Document = 0b00001,
    Declaration = 0b00010,
    Statement = 0b00100,
    Expression = 0b01000,
    Pattern = 0b10000,
    All = Document | Declaration | Statement | Expression | Pattern,
}
