using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Binding;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics;

public sealed class SemanticTree
{
    public SyntaxTree Syntax { get; }

    public DocumentSemantics Root { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    private SemanticTree(SyntaxTree syntax, DocumentSemantics root, ImmutableArray<Diagnostic> diagnostics)
    {
        Syntax = syntax;
        Root = root;
        Diagnostics = diagnostics;

        root.SetParent(this);
    }

    public static SemanticTree Analyze(
        SyntaxTree syntax, InteractiveContext? context, params DiagnosticAnalyzer[] analyzers)
    {
        return Analyze(syntax, context, analyzers.AsEnumerable());
    }

    public static SemanticTree Analyze(
        SyntaxTree syntax, InteractiveContext? context, IEnumerable<DiagnosticAnalyzer> analyzers)
    {
        Check.Null(syntax);
        Check.Argument((syntax.Root, context) is (InteractiveDocumentSyntax, _) or (_, null), context);
        Check.Null(analyzers);
        Check.All(analyzers, static analyzer => analyzer != null);

        var diags = ImmutableArray.CreateBuilder<Diagnostic>(0);
        var root = new LanguageAnalyzer(syntax, context ?? InteractiveContext.Default, diags).Analyze();

        foreach (var analyzer in analyzers)
        {
            var ctx = new DiagnosticAnalyzerContext(root, diags);

            analyzer.Analyze(ctx);
            ctx.Invalidate();
        }

        diags.Sort(static (x, y) => x.Span.CompareTo(y.Span));

        return new(syntax, root, diags.DrainToImmutable());
    }
}
