namespace Vezel.Celerity.Semantics;

internal sealed class LanguageAnalyzer : SyntaxWalker<object?>
{
    private readonly SemanticTablesBuilder _builder;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly Scope _scope;

    public LanguageAnalyzer(
        RootScope scope, SemanticTablesBuilder builder, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _scope = scope;
        _builder = builder;
        _diagnostics = diagnostics;

        // TODO

        _ = _scope;
        _ = _builder;
        _ = _diagnostics;
    }
}
