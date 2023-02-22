namespace Vezel.Celerity.Quality;

public sealed class LintAnalysis
{
    public SemanticAnalysis Semantics { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private LintAnalysis(SemanticAnalysis semantics, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Semantics = semantics;
        Diagnostics = diagnostics;
    }

    public static LintAnalysis Create(
        SemanticAnalysis semantics, IEnumerable<LintPass> passes, LintConfiguration configuration)
    {
        Check.Null(semantics);
        Check.Null(passes);
        Check.All(passes, static pass => pass != null);
        Check.Null(configuration);

        var diags = semantics.Diagnostics.ToBuilder();

        new LanguageLinter(semantics.Document, passes, configuration, diags).Lint();

        return new(semantics, diags.DrainToImmutable());
    }
}
