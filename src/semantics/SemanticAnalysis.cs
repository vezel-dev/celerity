namespace Vezel.Celerity.Semantics;

public sealed class SemanticAnalysis
{
    public SyntaxTree Tree { get; }

    public ImmutableArray<Declaration> Declarations { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasDiagnostics => !Diagnostics.IsEmpty;

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SemanticAnalysis(
        SyntaxTree tree, ImmutableArray<Declaration> declarations, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Tree = tree;
        Declarations = declarations;
        Diagnostics = diagnostics;
    }

    public static SemanticAnalysis Analyze(SyntaxTree tree)
    {
        Check.Null(tree);

        var decls = ImmutableArray.CreateBuilder<Declaration>();
        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        diags.AddRange(tree.Diagnostics);

        _ = new LanguageAnalyzer(decls, diags).VisitNode(tree.Root, null);

        return new(tree, decls.ToImmutable(), diags.ToImmutable());
    }
}
