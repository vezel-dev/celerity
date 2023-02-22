namespace Vezel.Celerity.Semantics;

public sealed partial class ModulePath : IEquatable<ModulePath>, IEqualityOperators<ModulePath, ModulePath, bool>
{
    public ImmutableArray<string> Components { get; }

    public string FullPath { get; }

    public ModulePath(params string[] components)
        : this(components.AsEnumerable())
    {
    }

    public ModulePath(IEnumerable<string> components)
    {
        Check.Null(components);
        Check.All(components, static component => ComponentRegex().IsMatch(component));

        Components = components.ToImmutableArray();
        FullPath = string.Join("::", components);
    }

    public static bool operator ==(ModulePath? left, ModulePath? right) =>
        EqualityComparer<ModulePath>.Default.Equals(left, right);

    public static bool operator !=(ModulePath? left, ModulePath? right) => !(left == right);

    public bool Equals(ModulePath? other)
    {
        return FullPath == other?.FullPath;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ModulePath);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FullPath);
    }

    public override string ToString()
    {
        return FullPath;
    }

    [GeneratedRegex(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ComponentRegex();
}
