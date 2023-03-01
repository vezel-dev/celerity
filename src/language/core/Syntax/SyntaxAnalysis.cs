using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

public sealed class SyntaxAnalysis
{
    // TODO: We should eventually get rid of this. When we need the source text, we can reconstruct it from the tree.
    public SourceText Text { get; }

    public DocumentSyntax Document { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SyntaxAnalysis(SourceText text, DocumentSyntax document, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Text = text;
        Document = document;
        Diagnostics = diagnostics;

        document.SetParent(this);
    }

    public static SyntaxAnalysis Create(SourceText text, SyntaxMode mode)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        return new(
            text,
            new LanguageParser(
                text,
                new LanguageLexer(
                    text,
                    mode,
                    diags).Lex(),
                mode,
                diags).ParseDocument(),
            diags.DrainToImmutable());
    }
}
