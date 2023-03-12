namespace Vezel.Celerity.Language.Text;

public static class TextFacts
{
    public static bool IsWhiteSpace(char value)
    {
        return value is '\f' or '\t' or '\v' || char.GetUnicodeCategory(value) == UnicodeCategory.SpaceSeparator;
    }

    public static bool IsNewLine(char value)
    {
        return value is '\n' or '\r' or '\u0085' ||
            char.GetUnicodeCategory(value) is UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator;
    }
}
