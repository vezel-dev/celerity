namespace Vezel.Celerity.Semantics;

public sealed class Module : AttributeTarget
{
    public ModuleNode Syntax { get; }

    public ModulePath Path { get; }

    internal Module(ModuleNode syntax)
        : base(syntax.Attributes)
    {
        Syntax = syntax;
        Path = new(syntax.Path.ComponentTokens.Items.Where(tok => !tok.IsMissing).Select(tok => tok.Text));
    }
}
