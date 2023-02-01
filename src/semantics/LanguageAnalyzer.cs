namespace Vezel.Celerity.Semantics;

internal sealed class LanguageAnalyzer : SyntaxWalker<object?>
{
    private readonly ImmutableArray<Declaration>.Builder _declarations;

    private readonly ImmutableArray<LambdaFunction>.Builder _lambdas;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    public LanguageAnalyzer(
        ImmutableArray<Declaration>.Builder declarations,
        ImmutableArray<LambdaFunction>.Builder lambdas,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _declarations = declarations;
        _lambdas = lambdas;
        _diagnostics = diagnostics;

        // TODO

        _ = _declarations;
        _ = _lambdas;
        _ = _diagnostics;
    }
}
