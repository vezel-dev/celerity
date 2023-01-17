namespace Vezel.Celerity.Syntax;

public sealed class SyntaxTree
{
    public SyntaxMode Mode { get; }

    public SyntaxNode Root { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasDiagnostics => !Diagnostics.IsEmpty;

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SyntaxTree(SyntaxMode mode, SyntaxNode root, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Mode = mode;
        Root = root;
        Diagnostics = diagnostics;
    }

    public static SyntaxTree Parse(SourceText text, SyntaxMode mode)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        return new(
            mode,
            new LanguageParser(
                new LanguageLexer(
                    text.FullPath,
                    text.EnumerateRunes().ToArray(),
                    mode,
                    diags).Lex(),
                mode,
                diags).Parse(),
            diags.ToImmutable());
    }
}
