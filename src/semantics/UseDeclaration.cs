namespace Vezel.Celerity.Semantics;

public sealed class UseDeclaration : Declaration
{
    public new UseDeclarationNode Syntax => Unsafe.As<UseDeclarationNode>(base.Syntax);

    public ModulePath Path { get; }

    internal UseDeclaration(UseDeclarationNode syntax)
        : base(syntax)
    {
        Path = new(syntax.Path.ComponentTokens.Items.Where(tok => !tok.IsMissing).Select(tok => tok.Text));
    }
}
