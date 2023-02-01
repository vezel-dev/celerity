namespace Vezel.Celerity.Semantics;

public abstract class Declaration : AttributeTarget
{
    public DeclarationNode Syntax { get; }

    public string Name => Syntax.NameToken.Text;

    private protected Declaration(DeclarationNode syntax)
        : base(syntax.Attributes)
    {
        Syntax = syntax;
    }
}
