namespace Vezel.Celerity.Semantics;

public sealed class VariableSymbol : Symbol
{
    public new PatternVariableBindingNode Syntax => Unsafe.As<PatternVariableBindingNode>(base.Syntax);

    public new PatternScope Scope => Unsafe.As<PatternScope>(base.Scope);

    public override string Name => Syntax.NameToken.Text;

    public override bool IsMutable => Syntax.MutKeywordToken != null;

    internal VariableSymbol(PatternVariableBindingNode syntax, PatternScope scope)
        : base(syntax, scope)
    {
    }
}
