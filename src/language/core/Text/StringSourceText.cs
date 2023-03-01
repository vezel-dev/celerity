namespace Vezel.Celerity.Language.Text;

public sealed class StringSourceText : SourceText
{
    public string Value { get; }

    public override int Count => Value.Length;

    public override char this[int index] => Value[index];

    private static readonly UnicodeEncoding _encoding = new(false, false, true);

    public StringSourceText(string path, string value)
        : base(path)
    {
        Check.NullOrEmpty(path);

        try
        {
            _ = _encoding.GetByteCount(value);
        }
        catch (EncoderFallbackException)
        {
            throw new ArgumentException(null, nameof(value));
        }

        Value = value;
    }

    public override CharEnumerator GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    public override string ToString(SourceTextSpan span)
    {
        return Value.Substring(span.Start, span.Length);
    }
}
