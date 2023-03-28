using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language;

public sealed partial class ModulePath : IEquatable<ModulePath>, IEqualityOperators<ModulePath, ModulePath, bool>
{
    public ImmutableArray<string> Components { get; }

    public string FullPath { get; }

    public ModulePath(params string[] components)
        : this(components.AsEnumerable())
    {
    }

    [SuppressMessage("", "CA1851")]
    public ModulePath(IEnumerable<string> components)
    {
        Check.Null(components);
        Check.Argument(components.Any(), components);
        Check.All(components, static comp => comp != null && SyntaxFacts.IsUpperIdentifier(comp));

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
