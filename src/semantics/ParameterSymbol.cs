namespace Vezel.Celerity.Semantics;

public sealed class ParameterSymbol : Symbol
{
    public Parameter Parameter { get; }

    public new CodeParameterNode Syntax => Unsafe.As<CodeParameterNode>(base.Syntax);

    public override string Name => Syntax.NameToken.Text;

    internal ParameterSymbol(Parameter parameter)
        : base(parameter.Syntax)
    {
        Parameter = parameter;
    }
}
