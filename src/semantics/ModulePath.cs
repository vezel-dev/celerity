namespace Vezel.Celerity.Semantics;

public sealed class ModulePath : IEquatable<ModulePath>, IEqualityOperators<ModulePath, ModulePath, bool>
{
    private const string PathStartChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private const string PathChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string CoreModuleName { get; } = "Core";

    public ImmutableArray<string> Components { get; }

    public string FullPath { get; }

    public bool IsCore => Components[0] == CoreModuleName;

    public ModulePath(params string[] components)
        : this(components.AsEnumerable())
    {
    }

    public ModulePath(IEnumerable<string> components)
    {
        Check.Null(components);
        Check.All(
            components,
            static component =>
                component.Length != 0 &&
                PathStartChars.AsSpan().Contains(component[0]) &&
                component.AsSpan(1).IndexOfAnyExcept(PathChars) == -1);

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
}
