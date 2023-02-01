namespace Vezel.Celerity.Semantics;

public sealed class Parameter : AttributeTarget
{
    public CodeParameterNode Syntax { get; }

    public int Ordinal { get; }

    public string Name => Syntax.NameToken.Text;

    internal Parameter(CodeParameterNode syntax, int ordinal)
        : base(syntax.Attributes)
    {
        Syntax = syntax;
        Ordinal = ordinal;
    }
}
