namespace Vezel.Celerity.Syntax;

public sealed class Utf8SourceText : SourceText
{
    public ReadOnlyMemory<byte> Text { get; }

    private Utf8SourceText(string fullPath, ReadOnlyMemory<byte> text)
        : base(fullPath)
    {
        Text = text;
    }

    public static Utf8SourceText From(string fullPath, ReadOnlySpan<byte> text)
    {
        return new(fullPath, text.ToArray());
    }

    public static Utf8SourceText From(string fullPath, ReadOnlySpan<char> text)
    {
        var utf8 = new byte[Encoding.GetByteCount(text)];

        _ = Encoding.GetBytes(text, utf8);

        return new(fullPath, utf8);
    }

    public static async Task<Utf8SourceText> FromAsync(
        string fullPath, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var ms = new MemoryStream();

        await using (ms.ConfigureAwait(false))
        {
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);

            return new(fullPath, ms.ToArray());
        }
    }

    public override IEnumerable<Rune> EnumerateRunes()
    {
        var remaining = Text;

        while (!remaining.IsEmpty)
        {
            yield return Rune.DecodeFromUtf8(remaining.Span, out var rune, out var consumed) == OperationStatus.Done
                ? rune
                : throw new InvalidDataException();

            remaining = remaining[consumed..];
        }
    }
}
