namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class DiagnosticConfiguration
{
    public int ContextLines { get; private set; } = 3;

    public int TabWidth { get; private set; } = 4;

    public Func<Rune, int> WidthMeasurer { get; private set; } = static rune => 1;

    public DiagnosticStyle Style { get; private set; } = PlainDiagnosticStyle.Instance;

    private DiagnosticConfiguration Clone()
    {
        return new()
        {
            ContextLines = ContextLines,
            TabWidth = TabWidth,
            WidthMeasurer = WidthMeasurer,
            Style = Style,
        };
    }

    public DiagnosticConfiguration WithContextLines(int contextLines)
    {
        Check.Range(contextLines >= 0, contextLines);

        var cfg = Clone();

        cfg.ContextLines = contextLines;

        return cfg;
    }

    public DiagnosticConfiguration WithTabWidth(int tabWidth)
    {
        Check.Range(tabWidth >= 1, tabWidth);

        var cfg = Clone();

        cfg.TabWidth = tabWidth;

        return cfg;
    }

    public DiagnosticConfiguration WithWidthMeasurer(Func<Rune, int> widthMeasurer)
    {
        Check.Null(widthMeasurer);

        var cfg = Clone();

        cfg.WidthMeasurer = widthMeasurer;

        return cfg;
    }

    public DiagnosticConfiguration WithStyle(DiagnosticStyle style)
    {
        Check.Null(style);

        var cfg = Clone();

        cfg.Style = style;

        return cfg;
    }
}
