namespace Vezel.Celerity.Semantics;

public sealed class DeclarationSymbol : Symbol
{
    public CodeDeclaration Declaration { get; }

    public new CodeDeclarationNode Syntax => Unsafe.As<CodeDeclarationNode>(base.Syntax);

    public override string Name => Declaration.Name;

    internal DeclarationSymbol(CodeDeclaration declaration)
        : base(declaration.Syntax)
    {
        Declaration = declaration;
    }
}
