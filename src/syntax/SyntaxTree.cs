using Vezel.Celerity.Syntax.Text;
using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax;

public sealed class SyntaxTree
{
    public DocumentSyntax Document { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private SyntaxTree(DocumentSyntax document, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Document = document;
        Diagnostics = diagnostics;
    }

    public static SyntaxTree Parse(SourceText text, SyntaxMode mode)
    {
        Check.Null(text);
        Check.Enum(mode);

        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        return new(
            new LanguageParser(
                text.Path,
                new LanguageLexer(
                    text,
                    mode,
                    diags).Lex(),
                mode,
                diags).ParseDocument(),
            diags.ToImmutable());
    }
}
