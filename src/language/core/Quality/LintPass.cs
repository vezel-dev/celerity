using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Quality.Passes;

namespace Vezel.Celerity.Language.Quality;

public abstract class LintPass
{
    public static ImmutableArray<LintPass> DefaultPasses { get; } =
        ImmutableArray.Create<LintPass>(
            TestWithoutAssertPass.Instance,
            UndocumentedPublicDeclarationPass.Instance,
            UnreachableCodePass.Instance,
            UnusedLocalSymbolPass.Instance,
            UppercaseBaseIndicatorPass.Instance);

    public DiagnosticCode Code { get; }

    protected LintPass(string code)
    {
        Code = DiagnosticCode.Create(code);
    }

    protected internal abstract void Run(LintPassContext context);
}
