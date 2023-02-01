namespace Vezel.Celerity.Semantics;

public sealed class LambdaFunction
{
    public LambdaExpressionNode Syntax { get; }

    public int Id { get; }

    public ImmutableArray<Parameter> Parameters { get; }

    public int Arity => Parameters.Length;

    internal LambdaFunction(LambdaExpressionNode syntax, int id)
    {
        Syntax = syntax;
        Id = id;
        Parameters =
            syntax.ParameterList.Parameters.Items.Select((param, i) => new Parameter(param, i)).ToImmutableArray();
    }
}
