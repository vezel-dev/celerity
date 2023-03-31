using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UppercaseBaseIndicatorPass : LintPass
{
    public static UppercaseBaseIndicatorPass Instance { get; } = new();

    private UppercaseBaseIndicatorPass()
        : base("uppercase-base-indicator")
    {
    }

    [SuppressMessage("", "CA1308")]
    protected internal override void Run(LintPassContext context)
    {
        foreach (var token in context.Root.Syntax.DescendantTokens())
        {
            var text = token.Text;

            if (token.Kind == SyntaxTokenKind.IntegerLiteral &&
                text.Length >= 2 &&
                text.AsSpan(0, 2) is "0B" or "0O" or "0X")
                context.ReportDiagnostic(
                    token.Span,
                    $"Consider using lowercase base indicator '{text[..2].ToLowerInvariant()}' for clarity");
        }
    }
}
