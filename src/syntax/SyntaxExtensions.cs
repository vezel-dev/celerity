namespace Vezel.Celerity.Syntax;

public static class SyntaxExtensions
{
    public static IEnumerable<SyntaxNode> Ancestors(this SyntaxNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var current = node;

        while ((current = current.Parent) != null)
            yield return current;
    }

    public static IEnumerable<SyntaxNode> Siblings(this SyntaxNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var parent = node.Parent;

        if (parent == null)
            yield break;

        foreach (var child in parent.Children().Where(n => n != node))
            yield return child;
    }

    public static IEnumerable<SyntaxNode> Descendants(this SyntaxNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var work = new Queue<SyntaxNode>();

        work.Enqueue(node);

        while (work.Count != 0)
        {
            var current = work.Dequeue();

            if (!current.HasChildren)
                continue;

            foreach (var child in current.Children())
            {
                yield return child;

                work.Enqueue(child);
            }
        }
    }
}
