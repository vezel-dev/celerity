namespace Vezel.Celerity.Syntax;

public abstract class SyntaxNode : SyntaxItem
{
    public new SyntaxNode? Parent => Unsafe.As<SyntaxNode?>(base.Parent);

    public override bool HasChildren => HasNodes || HasTokens;

    public abstract bool HasNodes { get; }

    public abstract bool HasTokens { get; }

    private protected SyntaxNode()
    {
    }

    public new IEnumerable<SyntaxNode> Ancestors()
    {
        return base.Ancestors().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> AncestorsAndSelf()
    {
        return base.AncestorsAndSelf().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> Siblings()
    {
        return base.Siblings().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> SiblingsAndSelf()
    {
        return base.SiblingsAndSelf().UnsafeCast<SyntaxNode>();
    }

    public IEnumerable<SyntaxNode> ChildNodes()
    {
        return Children().OfType<SyntaxNode>();
    }

    public IEnumerable<SyntaxToken> ChildTokens()
    {
        return Children().OfType<SyntaxToken>();
    }

    public override IEnumerable<SyntaxItem> Descendants()
    {
        var work = new Queue<SyntaxItem>();

        work.Enqueue(this);

        do
        {
            var current = work.Dequeue();

            if (current.HasChildren)
                foreach (var child in current.Children())
                    work.Enqueue(child);

            yield return current;
        }
        while (work.Count != 0);
    }

    // TODO: Optimize some of these (e.g. avoid descending into trivia and tokens when possible).

    public IEnumerable<SyntaxNode> DescendantNodes()
    {
        return Descendants().OfType<SyntaxNode>();
    }

    public IEnumerable<SyntaxToken> DescendantTokens()
    {
        return Descendants().OfType<SyntaxToken>();
    }

    public IEnumerable<SyntaxTrivia> DescendantTrivia()
    {
        return Descendants().OfType<SyntaxTrivia>();
    }

    internal abstract T Visit<T>(SyntaxWalker<T> walker, T state);
}
