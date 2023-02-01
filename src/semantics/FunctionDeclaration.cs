namespace Vezel.Celerity.Semantics;

public sealed class FunctionDeclaration : CodeDeclaration
{
    public new FunctionDeclarationNode Syntax => Unsafe.As<FunctionDeclarationNode>(base.Syntax);

    public new BlockExpressionNode? Body => Unsafe.As<BlockExpressionNode?>(base.Body);

    [MemberNotNullWhen(false, nameof(Body))]
    public bool IsExternal => Syntax.ExtKeywordToken != null;

    public ImmutableArray<Parameter> Parameters { get; }

    public int Arity => Parameters.Length;

    internal FunctionDeclaration(FunctionDeclarationNode syntax)
        : base(syntax)
    {
        Parameters =
            syntax.ParameterList.Parameters.Items.Select((param, i) => new Parameter(param, i)).ToImmutableArray();
    }
}
