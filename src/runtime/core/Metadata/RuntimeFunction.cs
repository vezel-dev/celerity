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
        : this(module, isPublic: false, $"λ{module.AllocateLambdaId()}", attributes: [], function.Parameters)
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

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (IsPublic)
            _ = sb.Append("pub ");

        if (IsExternal)
            _ = sb.Append("ext ");

        if (IsFallible)
            _ = sb.Append("err ");

        _ = sb.Append(CultureInfo.InvariantCulture, $"fn {Module.Path}.{Name}");

        return sb.ToString();
    }
}
