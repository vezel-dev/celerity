using Vezel.Celerity.Semantics.Binding;
using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics;

public sealed class SemanticTree
{
    public SyntaxTree Syntax { get; }

    public DocumentSemantics Document { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SemanticTree(SyntaxTree syntax, DocumentSemantics document, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Syntax = syntax;
        Document = document;
        Diagnostics = diagnostics;
    }

    public static SemanticTree Analyze(SyntaxTree syntax)
    {
        Check.Null(syntax);

        var root = syntax.Document;
        var scope = new Scope(null);
        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        diags.AddRange(syntax.Diagnostics);

        _ = new LanguageAnalyzer(scope, diags).VisitNode(root, null!); // TODO

        return new(syntax, null!, diags.ToImmutable());
    }
}
