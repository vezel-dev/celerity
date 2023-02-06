namespace Vezel.Celerity.Semantics;

public sealed class DeclarationSymbol : Symbol
{
    public new CodeDeclarationNode Syntax => Unsafe.As<CodeDeclarationNode>(base.Syntax);

    public new RootScope Scope => Unsafe.As<RootScope>(base.Scope);

    public CodeDeclaration Declaration { get; }

    public override string Name => Declaration.Name;

    internal DeclarationSymbol(CodeDeclaration declaration, RootScope scope)
        : base(declaration.Syntax, scope)
    {
        Declaration = declaration;
    }
}
