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

    public static SemanticTree Analyze(SyntaxTree syntax, InteractiveContext? context = null)
    {
        Check.Null(syntax);
        Check.Argument((syntax.Root, context) is (InteractiveDocumentSyntax, _) or (_, null), context);

        var diags = ImmutableArray.CreateBuilder<Diagnostic>(0);
        var root = new LanguageAnalyzer(syntax, context ?? InteractiveContext.Default, diags).Analyze();

        diags.Sort(static (x, y) => x.Span.CompareTo(y.Span));

        return new(syntax, root, diags.DrainToImmutable());
    }
}
