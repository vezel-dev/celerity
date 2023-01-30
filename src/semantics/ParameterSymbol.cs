namespace Vezel.Celerity.Semantics;

public sealed class ParameterSymbol : Symbol
{
    public new CodeParameterNode Syntax => Unsafe.As<CodeParameterNode>(base.Syntax);

    public override string Name => Syntax.NameToken.Text;

    internal ParameterSymbol(CodeParameterNode syntax)
        : base(syntax)
    {
    }
}
