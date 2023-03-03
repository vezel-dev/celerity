using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics;

namespace Vezel.Celerity.Language.Quality;

public sealed class LintAnalysis
{
    public SemanticTree Semantics { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private LintAnalysis(SemanticTree semantics, ImmutableArray<Diagnostic> diagnostics)
    {
        Semantics = semantics;
        Diagnostics = diagnostics;
    }

    public static LintAnalysis Create(
        SemanticTree semantics, IEnumerable<LintPass> passes, LintConfiguration configuration)
    {
        Check.Null(semantics);
        Check.Null(passes);
        Check.All(passes, static pass => pass != null);
        Check.Null(configuration);

        var diags = ImmutableArray.CreateBuilder<Diagnostic>(0);

        new LanguageLinter(semantics, passes, configuration, diags).Lint();

        diags.Sort((x, y) => x.Span.CompareTo(y.Span));

        return new(semantics, diags.DrainToImmutable());
    }
}
