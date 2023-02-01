namespace Vezel.Celerity.Semantics;

public abstract class AttributeTarget
{
    public ImmutableArray<AttributePair> Attributes { get; }

    private protected AttributeTarget(SyntaxItemList<AttributeNode> syntax)
    {
        Attributes = syntax.Select(attr => new AttributePair(attr)).ToImmutableArray();
    }
}
