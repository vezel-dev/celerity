// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxTerminal : SyntaxItem
{
    [SuppressMessage("", "CA1721")]
    public string Text { get; }

    private protected SyntaxTerminal(string text)
    {
        Text = text;
    }

    public abstract override IEnumerable<SyntaxTerminal> Children();

    public new IEnumerable<SyntaxTerminal> ChildrenAndSelf()
    {
        return base.ChildrenAndSelf().UnsafeCast<SyntaxTerminal>();
    }

    public abstract override IEnumerable<SyntaxTerminal> Descendants();

    public new IEnumerable<SyntaxTerminal> DescendantsAndSelf()
    {
        return base.DescendantsAndSelf().UnsafeCast<SyntaxTerminal>();
    }

    public override sealed string ToString()
    {
        return Text;
    }
}
