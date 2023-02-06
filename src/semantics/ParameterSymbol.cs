namespace Vezel.Celerity.Semantics;

public sealed class ParameterSymbol : Symbol
{
    public new CodeParameterNode Syntax => Unsafe.As<CodeParameterNode>(base.Syntax);

    public new CallableScope Scope => Unsafe.As<CallableScope>(base.Syntax);

    public Parameter Parameter { get; }

    public override string Name => Syntax.NameToken.Text;

    internal ParameterSymbol(Parameter parameter, CallableScope scope)
        : base(parameter.Syntax, scope)
    {
        Parameter = parameter;
    }
}
