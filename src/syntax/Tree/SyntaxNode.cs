namespace Vezel.Celerity.Syntax.Tree;

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

    public abstract IEnumerable<SyntaxNode> ChildNodes();

    public abstract IEnumerable<SyntaxToken> ChildTokens();

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

    public IEnumerable<SyntaxNode> DescendantNodes()
    {
        var work = new Queue<SyntaxNode>();

        work.Enqueue(this);

        do
        {
            var current = work.Dequeue();

            if (current.HasNodes)
                foreach (var child in current.ChildNodes())
                    work.Enqueue(child);

            yield return current;
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxToken> DescendantTokens()
    {
        // Not much we can do better here.
        return Descendants().OfType<SyntaxToken>();
    }

    public IEnumerable<SyntaxTrivia> DescendantTrivia()
    {
        foreach (var token in DescendantTokens())
        {
            foreach (var leading in token.LeadingTrivia)
                yield return leading;

            foreach (var trailing in token.TrailingTrivia)
                yield return trailing;
        }
    }

    internal abstract T Visit<T>(SyntaxWalker<T> walker, T state);
}
