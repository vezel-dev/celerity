// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxToken : SyntaxTerminal
{
    public new SyntaxNode Parent => Unsafe.As<SyntaxNode>(base.Parent!);

    public override SourceTextSpan Span => !IsMissing ? new(_position, Text.Length) : SourceTextSpan.Empty;

    public override SourceTextSpan FullSpan
    {
        get
        {
            if (IsMissing)
                return SourceTextSpan.Empty;

            var start = LeadingTrivia.Count != 0 ? LeadingTrivia[0].FullSpan.Start : _position;
            var length = TrailingTrivia.Count != 0 ? TrailingTrivia[^1].FullSpan.End - start : Text.Length;

            return new(start, length);
        }
    }

    public override bool HasChildren => !LeadingTrivia.IsEmpty || !TrailingTrivia.IsEmpty;

    public SyntaxTokenKind Kind { get; }

    public bool IsMissing => Kind == SyntaxTokenKind.Missing;

    public bool IsEndOfInput => Kind == SyntaxTokenKind.EndOfInput;

    public object? Value { get; }

    public SyntaxItemList<SyntaxTrivia> LeadingTrivia { get; }

    public SyntaxItemList<SyntaxTrivia> TrailingTrivia { get; }

    private readonly int _position;

    internal SyntaxToken()
        : this(
            position: -1, SyntaxTokenKind.Missing, text: string.Empty, value: null, leading: new([]), trailing: new([]))
    {
    }

    internal SyntaxToken(
        int position,
        SyntaxTokenKind kind,
        string text,
        object? value,
        SyntaxItemList<SyntaxTrivia> leading,
        SyntaxItemList<SyntaxTrivia> trailing)
        : base(text)
    {
        _position = position;
        Kind = kind;
        Value = value;
        LeadingTrivia = new(leading, this);
        TrailingTrivia = new(trailing, this);
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

    public override string ToFullString()
    {
        return LeadingTrivia.IsEmpty && TrailingTrivia.IsEmpty ? ToString() : base.ToFullString();
    }

    internal override void ToString(StringBuilder builder, bool leading, bool trailing)
    {
        if (leading)
            foreach (var trivia in LeadingTrivia)
                _ = builder.Append(trivia);

        _ = builder.Append(Text);

        if (trailing)
            foreach (var trivia in TrailingTrivia)
                _ = builder.Append(trivia);
    }
}
