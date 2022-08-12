namespace Vezel.Celerity.Syntax;

public abstract class SyntaxItem
{
    public abstract SyntaxItem? Parent { get; }

    private protected SyntaxItem()
    {
    }
}
