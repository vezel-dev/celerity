using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language.Semantics;

public sealed class SemanticTree
{
    public SyntaxTree Syntax { get; }

    public DocumentSemantics Root { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SemanticTree(SyntaxTree syntax, DocumentSemantics root, ImmutableArray<Diagnostic> diagnostics)
    {
        Syntax = syntax;
        Root = root;
        Diagnostics = diagnostics;

        root.SetParent(this);
    }

    public static SemanticTree Analyze(SyntaxTree syntax)
    {
        Check.Null(syntax);

        var diags = ImmutableArray.CreateBuilder<Diagnostic>(0);

        return new(syntax, new LanguageAnalyzer(syntax, diags).Analyze(), diags.DrainToImmutable());
    }
}
