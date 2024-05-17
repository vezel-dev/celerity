// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class DiagnosticWriterConfiguration
{
    public int ContextLines { get; private set; } = 3;

    public int TabWidth { get; private set; } = 4;

    public Func<Rune, int> WidthMeasurer { get; private set; } = static rune => 1;

    public DiagnosticStyle Style { get; private set; } = PlainDiagnosticStyle.Instance;

    private DiagnosticWriterConfiguration Clone()
    {
        return new()
        {
            ContextLines = ContextLines,
            TabWidth = TabWidth,
            WidthMeasurer = WidthMeasurer,
            Style = Style,
        };
    }

    public DiagnosticWriterConfiguration WithContextLines(int contextLines)
    {
        Check.Range(contextLines >= 0, contextLines);

        var cfg = Clone();

        cfg.ContextLines = contextLines;

        return cfg;
    }

    public DiagnosticWriterConfiguration WithTabWidth(int tabWidth)
    {
        Check.Range(tabWidth >= 1, tabWidth);

        var cfg = Clone();

        cfg.TabWidth = tabWidth;

        return cfg;
    }

    public DiagnosticWriterConfiguration WithWidthMeasurer(Func<Rune, int> widthMeasurer)
    {
        Check.Null(widthMeasurer);

        var cfg = Clone();

        cfg.WidthMeasurer = widthMeasurer;

        return cfg;
    }

    public DiagnosticWriterConfiguration WithStyle(DiagnosticStyle style)
    {
        Check.Null(style);

        var cfg = Clone();

        cfg.Style = style;

        return cfg;
    }
}
