namespace Vezel.Celerity.Language.Syntax.Tree;

public enum SyntaxTriviaKind
{
    ShebangLine,
    WhiteSpace,
    NewLine,
    Comment,
    SkippedToken, // Synthesized in LanguageParser when attempting to recover from parse errors.
}
