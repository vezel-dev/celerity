namespace Vezel.Celerity.Syntax;

public abstract class SyntaxItem
{
    public SyntaxItem? Parent { get; private set; }

    internal void SetParent(SyntaxItem parent)
    {
        Parent = parent;
    }

    private protected SyntaxItem()
    {
    }
}
