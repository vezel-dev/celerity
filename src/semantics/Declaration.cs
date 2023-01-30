namespace Vezel.Celerity.Semantics;

public abstract class Declaration
{
    public DeclarationNode Syntax { get; }

    public string Name => Syntax.NameToken.Text;

    private protected Declaration(DeclarationNode syntax)
    {
        Syntax = syntax;
    }
}
