namespace Vezel.Celerity.Syntax;

public class SourceText
{
    public string FullPath { get; }

    public ReadOnlyMemory<Rune> Runes { get; }

    private static readonly UTF8Encoding _utf8 = new(false, true);

    private static readonly UnicodeEncoding _utf16 = new(false, false, true);

    protected SourceText(string fullPath, scoped ReadOnlySpan<Rune> runes)
    {
        Check.NullOrEmpty(fullPath);

        FullPath = fullPath;
        Runes = runes.ToArray();
    }

    public static SourceText FromUtf8(string fullPath, scoped ReadOnlySpan<byte> text)
    {
        string str;

        try
        {
            str = _utf8.GetString(text);
        }
        catch (DecoderFallbackException)
        {
            throw new InvalidDataException();
        }

        return new(fullPath, str.EnumerateRunes().ToArray());
    }

    public static SourceText FromUtf16(string fullPath, scoped ReadOnlySpan<char> text)
    {
        var runes = text.ToString().EnumerateRunes().ToArray();

        Check.Data(runes.All(rune => rune != Rune.ReplacementChar));

        return new(fullPath, runes);
    }

    public static ValueTask<SourceText> FromUtf8Async(
        string fullPath, Stream stream, CancellationToken cancellationToken = default)
    {
        return FromAsync(fullPath, stream, _utf8, cancellationToken);
    }

    public static ValueTask<SourceText> FromUtf16Async(
        string fullPath, Stream stream, CancellationToken cancellationToken = default)
    {
        return FromAsync(fullPath, stream, _utf16, cancellationToken);
    }

    private static async ValueTask<SourceText> FromAsync(
        string fullPath, Stream stream, Encoding encoding, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, encoding, false, leaveOpen: true);

        string str;

        try
        {
            str = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DecoderFallbackException)
        {
            throw new InvalidDataException();
        }

        return new(fullPath, str.EnumerateRunes().ToArray());
    }
}
