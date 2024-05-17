// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Text;

public sealed class SourceTextLineList : IReadOnlyList<SourceTextLine>
{
    public struct Enumerator : IEnumerator<SourceTextLine>
    {
        public readonly SourceTextLine Current => _enumerator.Current;

        readonly object IEnumerator.Current => Current;

        private ImmutableArray<SourceTextLine>.Enumerator _enumerator;

        internal Enumerator(ImmutableArray<SourceTextLine>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

        readonly void IDisposable.Dispose()
        {
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        readonly void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public SourceText Text { get; }

    public int Count => _lines.Length;

    public SourceTextLine this[int index] => _lines[index];

    private readonly ImmutableArray<SourceTextLine> _lines;

    private readonly ReadOnlyMemory<int> _positions;

    internal SourceTextLineList(SourceText text, ImmutableArray<SourceTextLine> lines)
    {
        Text = text;
        _lines = lines;
        _positions = _lines.Select(static line => line.Span.Start).ToArray();
    }

    public int IndexOf(int position)
    {
        Check.Range(position >= 0 && position <= Text.Count, position);

        var index = _positions.Span.BinarySearch(position);

        // Consult the BinarySearch documentation to see why this apparent magic is done.
        if (index < 0)
            index = ~index - 1;

        return index;
    }

    public SourceTextLine GetLine(int position)
    {
        return this[IndexOf(position)];
    }

    public SourceTextLinePosition GetLinePosition(int position)
    {
        var line = GetLine(position);

        return new(line.Line, position - line.Span.Start);
    }

    public int GetPosition(SourceTextLinePosition position)
    {
        return this[position.Line].Span.Start + position.Character;
    }

    public Enumerator GetEnumerator()
    {
        return new(_lines.GetEnumerator());
    }

    IEnumerator<SourceTextLine> IEnumerable<SourceTextLine>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
