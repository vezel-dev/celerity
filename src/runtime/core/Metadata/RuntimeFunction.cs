namespace Vezel.Celerity.Runtime.Metadata;

public class RuntimeFunction : RuntimeMember
{
    public bool IsExternal { get; }

    public bool IsFallible { get; }

    public ImmutableArray<RuntimeParameter> Parameters { get; }

    internal RuntimeFunction(RuntimeModule module, FunctionDeclarationSemantics function)
        : this(module, function.IsPublic, function.Symbol!.Name, function.Attributes, function.Parameters)
    {
        IsExternal = function.IsExternal;
        IsFallible = function.IsFallible;
    }

    internal RuntimeFunction(RuntimeModule module, LambdaExpressionSemantics function)
        : this(
            module,
            isPublic: false,
            $"λ{module.AllocateLambdaId()}",
            Array.Empty<AttributeSemantics>(),
            function.Parameters)
    {
    }

    private RuntimeFunction(
        RuntimeModule module,
        bool isPublic,
        string name,
        IEnumerable<AttributeSemantics> attributes,
        IEnumerable<CodeParameterSemantics> parameters)
        : base(module, isPublic, name, attributes)
    {
        Parameters = parameters.Select((param, i) => new RuntimeParameter(this, i, param)).ToImmutableArray();
    }
}
