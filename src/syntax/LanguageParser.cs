namespace Vezel.Celerity.Syntax;

internal sealed class LanguageParser
{
    private readonly IReadOnlyList<SyntaxToken> _tokens;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    public LanguageParser(
        IReadOnlyList<SyntaxToken> tokens, SyntaxMode mode, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _tokens = tokens;
        _mode = mode;
        _diagnostics = diagnostics;
    }

    public RootNode Parse()
    {
        // TODO

        _ = _tokens;
        _ = _mode;
        _ = _diagnostics;

        throw new NotImplementedException();
    }
}
