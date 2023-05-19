namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeConstant : RuntimeMember
{
    internal RuntimeConstant(RuntimeModule module, ConstantDeclarationSemantics constant)
        : base(module, constant.IsPublic, constant.Symbol!.Name, constant.Attributes)
    {
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (IsPublic)
            _ = sb.Append("pub ");

        _ = sb.Append(CultureInfo.InvariantCulture, $"const {Module.Path}.{Name}");

        return sb.ToString();
    }
}
