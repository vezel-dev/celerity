using Vezel.Celerity.Language.Quality.Passes;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality;

public abstract class LintPass
{
    public static ImmutableArray<LintPass> DefaultPasses { get; } =
        ImmutableArray.Create<LintPass>(
            UppercaseBaseIndicatorPass.Instance);

    public SourceDiagnosticCode Code { get; }

    public SourceDiagnosticSeverity? Severity { get; }

    public LintTargets Targets { get; }

    public SyntaxMode? Mode { get; }

    protected LintPass(string code, SourceDiagnosticSeverity? severity, LintTargets targets, SyntaxMode? mode)
    {
        Check.Enum(severity);
        Check.Enum(mode);

        Code = SourceDiagnosticCode.Create(code);
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
