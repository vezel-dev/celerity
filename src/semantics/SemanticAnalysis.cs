namespace Vezel.Celerity.Semantics;

public sealed class SemanticAnalysis
{
    public SyntaxTree Tree { get; }

    public Module? Module { get; }

    public ImmutableArray<Declaration> Declarations { get; }

    public ImmutableArray<LambdaFunction> Lambdas { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SemanticAnalysis(
        SyntaxTree tree,
        Module? module,
        ImmutableArray<Declaration> declarations,
        ImmutableArray<LambdaFunction> lambdas,
        ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Tree = tree;
        Module = module;
        Declarations = declarations;
        Lambdas = lambdas;
        Diagnostics = diagnostics;
    }

    public static SemanticAnalysis Analyze(SyntaxTree tree)
    {
        Check.Null(tree);

        var decls = ImmutableArray.CreateBuilder<Declaration>();
        var lambdas = ImmutableArray.CreateBuilder<LambdaFunction>();
        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        diags.AddRange(tree.Diagnostics);

        _ = new LanguageAnalyzer(decls, lambdas, diags).VisitNode(tree.Root, null);

        return new(
            tree,
            tree.Root is ModuleNode syntax ? new(syntax) : null,
            decls.ToImmutable(),
            lambdas.ToImmutable(),
            diags.ToImmutable());
    }
}
