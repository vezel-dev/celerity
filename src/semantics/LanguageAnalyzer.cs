using Vezel.Celerity.Semantics.Binding;
using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics;

internal sealed class LanguageAnalyzer : SyntaxWalker<SemanticNode>
{
    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly Scope _scope;

    public LanguageAnalyzer(Scope scope, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _scope = scope;
        _diagnostics = diagnostics;

        // TODO
        _ = new BlockScope(scope);
        _ = new LambdaScope(scope);
        _ = new LoopScope(scope);

        _ = _scope;
        _ = _diagnostics;
    }
}
