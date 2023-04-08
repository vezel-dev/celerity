namespace Vezel.Celerity.Language.Text;

public static class TextFacts
{
    public static bool IsWhiteSpace(char value)
    {
        // Characters HT, VT, FF, SP, and category Zs. Inlined for performance.
        return value switch
        {
            '\t' or
            '\v' or
            '\f' or
            ' ' or
            '\u00a0' or
            '\u1680' or
            '\u2000' or
            '\u2001' or
            '\u2002' or
            '\u2003' or
            '\u2004' or
            '\u2005' or
            '\u2006' or
            '\u2007' or
            '\u2008' or
            '\u2009' or
            '\u200a' or
            '\u202f' or
            '\u205f' or
            '\u3000' => true,
            _ => false,
        };
    }

    public static bool IsNewLine(char value)
    {
        // Characters LF, CR, NEL, and categories Zl and Zp. Inlined for performance.
        return value switch
        {
            '\n' or
            '\r' or
            '\u0085' or
            '\u2028' or
            '\u2029' => true,
            _ => false,
        };
    }
}
