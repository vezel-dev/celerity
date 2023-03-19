using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

public sealed class SyntaxTree
{
    public string Path { get; }

    public DocumentSyntax Root { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    private SoftReference<SourceText> _text;

    private SyntaxTree(
        SourceText text, bool discardText, DocumentSyntax root, IEnumerable<Func<SyntaxTree, Diagnostic>> diagnostics)
    {
        _text = new(discardText ? null : text);
        Path = text.Path;
        Root = root;
        Diagnostics = diagnostics.Select(creator => creator(this)).OrderBy(diag => diag.Span).ToImmutableArray();

        root.SetParent(this);
    }

    public static SyntaxTree Parse(SourceText text, SyntaxMode mode, bool discardText = false)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = new List<Func<SyntaxTree, Diagnostic>>(0);

        return new(
            text,
            discardText,
            new LanguageParser(new LanguageLexer(text, mode, diags).Lex(), mode, diags).ParseDocument(),
            diags);
    }

    public SourceText GetText()
    {
        if ((SourceText?)_text is not { } text)
            _text = new(text = Root.GetFullText());

        return text;
    }
}
