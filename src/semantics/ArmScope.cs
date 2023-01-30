namespace Vezel.Celerity.Semantics;

public sealed class ArmScope : Scope
{
    public new ExpressionArmNode Syntax => Unsafe.As<ExpressionArmNode>(base.Syntax);

    internal ArmScope(ExpressionArmNode syntax, Scope? parent)
        : base(syntax, parent)
    {
    }
}
