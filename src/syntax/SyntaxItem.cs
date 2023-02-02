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

    public RootNode GetRoot()
    {
        // RootNode is the only syntax item with a null Parent.
        return Parent != null ? Parent.GetRoot() : Unsafe.As<RootNode>(this);
    }
}
