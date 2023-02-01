namespace Vezel.Celerity.Runtime.Metadata;

public class RuntimeFunction : RuntimeMember
{
    public bool IsExternal { get; }

    public ImmutableArray<RuntimeParameter> Parameters { get; }

    public int Arity => Parameters.Length;

    internal RuntimeFunction(RuntimeModule module, FunctionDeclaration function)
        : this(module, function.Name, function.Attributes, function.Parameters)
    {
        IsExternal = function.IsExternal;
    }

    internal RuntimeFunction(RuntimeModule module, LambdaFunction function)
        : this(module, $"λ{function.Id}", ImmutableArray<AttributePair>.Empty, function.Parameters)
    {
    }

    private RuntimeFunction(
        RuntimeModule module,
        string name,
        ImmutableArray<AttributePair> attributes,
        ImmutableArray<Parameter> parameters)
        : base(module, name, attributes)
    {
        Parameters = parameters.Select((param, i) => new RuntimeParameter(this, i, param)).ToImmutableArray();
    }
}
