using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Quality.Passes;
using Vezel.Celerity.Language.Syntax;

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

    public SyntaxMode? Mode { get; }

    protected LintPass(string code, SyntaxMode? mode)
    {
        Check.Enum(mode);

        Code = DiagnosticCode.Create(code);
        Mode = mode;
    }

    protected internal abstract void Run(LintPassContext context);
}
