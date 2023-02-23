namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeParameter : RuntimeMetadata
{
    public RuntimeFunction Function { get; }

    public int Ordinal { get; }

    public string Name { get; }

    internal RuntimeParameter(RuntimeFunction function, int ordinal, CodeParameterSemantics parameter)
        : base(parameter.Attributes)
    {
        Function = function;
        Ordinal = ordinal;
        Name = parameter.Symbol!.Name;
    }
}
