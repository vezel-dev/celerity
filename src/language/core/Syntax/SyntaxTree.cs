using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

public sealed class SyntaxTree
{
    // TODO: We should eventually get rid of this. When we need the source text, we can reconstruct it from the tree.
    public SourceText Text { get; }

    public DocumentSyntax Root { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SyntaxTree(SourceText text, DocumentSyntax root, IEnumerable<Func<SyntaxTree, Diagnostic>> diagnostics)
    {
        Text = text;
        Root = root;
        Diagnostics = diagnostics
            .Select(creator => creator(this))
            .OrderBy(diag => diag.Span)
            .ToImmutableArray();

        root.SetParent(this);
    }

    public static SyntaxTree Parse(SourceText text, SyntaxMode mode)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = new List<Func<SyntaxTree, Diagnostic>>(0);

        return new(
            text, new LanguageParser(new LanguageLexer(text, mode, diags).Lex(), mode, diags).ParseDocument(), diags);
    }
}
