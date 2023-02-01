namespace Vezel.Celerity.Semantics;

public sealed class AttributePair
{
    public AttributeNode Syntax { get; }

    public string Name => Syntax.NameToken.Text;

    public object? Value => Syntax.Value?.ValueToken.Value;

    internal AttributePair(AttributeNode syntax)
    {
        Syntax = syntax;
    }
}
