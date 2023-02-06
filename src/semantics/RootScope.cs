namespace Vezel.Celerity.Semantics;

public sealed class RootScope : Scope
{
    public new RootNode Syntax => Unsafe.As<RootNode>(base.Syntax);

    public RootScope(RootNode syntax)
        : base(syntax, null)
    {
    }

    internal bool TryDefineSymbol(CodeDeclaration declaration, [MaybeNullWhen(true)] out Symbol existing)
    {
        return TryDefineSymbol(new DeclarationSymbol(declaration, this), out existing);
    }
}
