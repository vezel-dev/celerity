using Vezel.Celerity.Language.Semantics.Binding;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics;

internal sealed class LanguageAnalyzer : SyntaxVisitor<SemanticNode>
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
        _ = new TryScope(scope);
        _ = new LoopScope(scope);

        _ = _scope;
        _ = _diagnostics;
    }
}
