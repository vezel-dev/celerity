namespace Vezel.Celerity.Runtime.Metadata;

public class RuntimeFunction : RuntimeMember
{
    public bool IsExternal { get; }

    public ImmutableArray<RuntimeParameter> Parameters { get; }

    public int Arity => Parameters.Length;

    internal RuntimeFunction(RuntimeModule module, FunctionDeclarationSemantics function)
        : this(module, function.Symbol!.Name, function.Attributes, function.Parameters)
    {
        IsExternal = function.IsExternal;
    }

    internal RuntimeFunction(RuntimeModule module, LambdaExpressionSemantics function)
        : this(module, $"λ{module.AllocateLambdaId()}", Array.Empty<AttributeSemantics>(), function.Parameters)
    {
    }

    private RuntimeFunction(
        RuntimeModule module,
        string name,
        IEnumerable<AttributeSemantics> attributes,
        IEnumerable<CodeParameterSemantics> parameters)
        : base(module, name, attributes)
    {
        Parameters = parameters.Select((param, i) => new RuntimeParameter(this, i, param)).ToImmutableArray();
    }
}
