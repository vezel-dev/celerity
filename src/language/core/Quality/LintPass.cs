using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Quality.Passes;
using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language.Quality;

public abstract class LintPass
{
    public static ImmutableArray<LintPass> DefaultPasses { get; } =
        ImmutableArray.Create<LintPass>(
            UndocumentedPublicDeclarationPass.Instance,
            UnreachableCodePass.Instance,
            UnusedLocalSymbolPass.Instance,
            UppercaseBaseIndicatorPass.Instance);

    public DiagnosticCode Code { get; }

    public DiagnosticSeverity Severity { get; }

    public SyntaxMode? Mode { get; }

    protected LintPass(string code, DiagnosticSeverity severity, SyntaxMode? mode)
    {
        Check.Enum(severity);
        Check.Argument(severity != DiagnosticSeverity.None, severity);
        Check.Enum(mode);

        Code = DiagnosticCode.Create(code);
        Severity = severity;
        Mode = mode;
    }

    protected internal abstract void Run(LintContext context);
}
