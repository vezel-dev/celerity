namespace Vezel.Celerity.Syntax;

public sealed class SyntaxTree
{
    public RootNode Root { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SyntaxTree(RootNode root, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Root = root;
        Diagnostics = diagnostics;
    }

    public static SyntaxTree Parse(SourceText text, SyntaxMode mode)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        return new(
            new LanguageParser(
                new LanguageLexer(
                    text,
                    mode,
                    diags).Lex(),
                mode,
                diags).Parse(),
            diags.ToImmutable());
    }
}
