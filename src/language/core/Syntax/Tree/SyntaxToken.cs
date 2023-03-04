using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxToken : SyntaxItem
{
    public new SyntaxNode Parent => Unsafe.As<SyntaxNode>(base.Parent!);

    public override SourceTextSpan Span => _position == -1 ? default : new(_position, Text.Length);

    public override SourceTextSpan FullSpan
    {
        get
        {
            if (_position == -1)
                return default;

            var start = LeadingTrivia.FirstOrDefault()?.FullSpan.Start ?? _position;
            var length = TrailingTrivia.LastOrDefault()?.FullSpan.End - start ?? Text.Length;

            return new(start, length);
        }
    }

    public override bool HasChildren => !LeadingTrivia.IsEmpty || !TrailingTrivia.IsEmpty;

    public SyntaxTokenKind Kind { get; }

    public bool IsMissing => Kind == SyntaxTokenKind.Missing;

    public bool IsEndOfInput => Kind == SyntaxTokenKind.EndOfInput;

    public bool IsUnrecognized => Kind == SyntaxTokenKind.Unrecognized;

    [SuppressMessage("", "CA1721")]
    public string Text { get; }

    public object? Value { get; }

    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    private readonly int _position;

    internal SyntaxToken()
        : this(
            -1,
            SyntaxTokenKind.Missing,
            "<error>",
            null,
            ImmutableArray<SyntaxTrivia>.Empty,
            ImmutableArray<SyntaxTrivia>.Empty)
    {
    }

    internal SyntaxToken(
        int position,
        SyntaxTokenKind kind,
        string text,
        object? value,
        ImmutableArray<SyntaxTrivia> leading,
        ImmutableArray<SyntaxTrivia> trailing)
    {
        _position = position;
        Kind = kind;
        Text = text;
        Value = value;
        LeadingTrivia = leading;
        TrailingTrivia = trailing;

        foreach (var trivia in leading)
            trivia.SetParent(this);

        foreach (var trivia in trailing)
            trivia.SetParent(this);
    }

    public new IEnumerable<SyntaxNode> Ancestors()
    {
        return base.Ancestors().UnsafeCast<SyntaxNode>();
    }

    public override IEnumerable<SyntaxTrivia> Children()
    {
        foreach (var leading in LeadingTrivia)
            yield return leading;

        foreach (var trailing in TrailingTrivia)
            yield return trailing;
    }

    public override IEnumerable<SyntaxTrivia> Descendants()
    {
        return Children();
    }

    public override string ToString()
    {
        return Text;
    }

    public override string ToFullString()
    {
        if (LeadingTrivia.IsEmpty && TrailingTrivia.IsEmpty)
            return ToString();

        var sb = new StringBuilder();

        foreach (var leading in LeadingTrivia)
            _ = sb.Append(leading);

        _ = sb.Append(Text);

        foreach (var trailing in TrailingTrivia)
            _ = sb.Append(trailing);

        return sb.ToString();
    }
}
