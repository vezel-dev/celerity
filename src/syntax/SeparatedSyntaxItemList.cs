namespace Vezel.Celerity.Syntax;

[SuppressMessage("", "CA1710")]
[SuppressMessage("", "CA1815")]
public readonly struct SeparatedSyntaxItemList<TItem, TSeparator> : IReadOnlyCollection<SyntaxItem>
    where TItem : SyntaxItem
    where TSeparator : SyntaxItem
{
    public SyntaxItemList<TItem> Items { get; }

    public SyntaxItemList<TSeparator> Separators { get; }

    public int Count => Items.Count + Separators.Count;

    internal SeparatedSyntaxItemList(SyntaxItemList<TItem> items, SyntaxItemList<TSeparator> separators)
    {
        Items = items;
        Separators = separators;
    }

    public IEnumerator<SyntaxItem> GetEnumerator()
    {
        var items = Items.GetEnumerator();
        var seps = Separators.GetEnumerator();

        while (items.MoveNext())
        {
            yield return items.Current;

            if (seps.MoveNext())
                yield return seps.Current;
        }

        // There can be a trailing separator.
        if (seps.MoveNext())
            yield return seps.Current;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
