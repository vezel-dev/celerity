namespace Vezel.Celerity.Syntax;

internal sealed class LanguageParser
{
    private readonly SyntaxInputReader<SyntaxToken> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    public LanguageParser(
        ReadOnlyMemory<SyntaxToken> tokens, SyntaxMode mode, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _reader = new(tokens);
        _mode = mode;
        _diagnostics = diagnostics;
    }

    public RootNode Parse()
    {
        // TODO

        _ = _reader;
        _ = _mode;
        _ = _diagnostics;

        throw new NotImplementedException();
    }
}
