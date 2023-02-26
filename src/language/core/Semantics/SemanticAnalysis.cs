using Vezel.Celerity.Language.Semantics.Binding;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics;

public sealed class SemanticAnalysis
{
    public SyntaxAnalysis Syntax { get; }

    public DocumentSemantics Document { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SemanticAnalysis(
        SyntaxAnalysis syntax, DocumentSemantics document, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Syntax = syntax;
        Document = document;
        Diagnostics = diagnostics;
    }

    public static SemanticAnalysis Create(SyntaxAnalysis syntax)
    {
        Check.Null(syntax);

        var root = syntax.Document;
        var scope = new Scope(null);
        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        diags.AddRange(syntax.Diagnostics);

        _ = new LanguageAnalyzer(scope, diags).VisitNode(root); // TODO

        return new(syntax, null!, diags.DrainToImmutable());
    }
}
