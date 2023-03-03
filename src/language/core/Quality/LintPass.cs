using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Quality.Passes;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language.Quality;

public abstract class LintPass
{
    public static ImmutableArray<LintPass> DefaultPasses { get; } =
        ImmutableArray.Create<LintPass>(
            UppercaseBaseIndicatorPass.Instance,
            UndocumentedPublicDeclarationPass.Instance);

    public DiagnosticCode Code { get; }

    public DiagnosticSeverity? Severity { get; }

    public LintTargets Targets { get; }

    public SyntaxMode? Mode { get; }

    protected LintPass(string code, DiagnosticSeverity? severity, LintTargets targets, SyntaxMode? mode)
    {
        Check.Enum(severity);
        Check.Enum(mode);

        Code = DiagnosticCode.Create(code);
        Severity = severity;
        Targets = targets;
        Mode = mode;
    }

    protected internal virtual void Run(LintContext context, DocumentSemantics document)
    {
    }

    protected internal virtual void Run(LintContext context, DeclarationSemantics declaration)
    {
    }

    protected internal virtual void Run(LintContext context, StatementSemantics statement)
    {
    }

    protected internal virtual void Run(LintContext context, ExpressionSemantics expression)
    {
    }

    protected internal virtual void Run(LintContext context, PatternSemantics pattern)
    {
    }
}
