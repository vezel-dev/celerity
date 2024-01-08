using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language;

public sealed class ModulePath : IEquatable<ModulePath>, IEqualityOperators<ModulePath, ModulePath, bool>
{
    public ImmutableArray<string> Components { get; }

    public string FullPath { get; }

    private ModulePath(ImmutableArray<string> components)
    {
        Components = components;
        FullPath = string.Join("::", components);
    }

    public static bool operator ==(ModulePath? left, ModulePath? right) =>
        EqualityComparer<ModulePath>.Default.Equals(left, right);

    public static bool operator !=(ModulePath? left, ModulePath? right) => !(left == right);

    public static ModulePath Create(params string[] components)
    {
        return Create(components.AsEnumerable());
    }

    public static ModulePath Create(IEnumerable<string> components)
    {
        Check.Argument(TryCreate(components, out var path), components);

        return path;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out ModulePath? path)
    {
        Check.Null(value);

        return TryCreate(value.Split("::"), out path);
    }

    [SuppressMessage("", "CA1851")]
    public static bool TryCreate(IEnumerable<string> components, [NotNullWhen(true)] out ModulePath? path)
    {
        Check.Null(components);
        Check.All(components, static comp => comp != null);

        var good = false;

        foreach (var comp in components)
        {
            if (!SyntaxFacts.IsUpperIdentifier(comp))
            {
                good = false;

                break;
            }

            good = true;
        }

        if (good)
        {
            path = new(components.ToImmutableArray());

            return true;
        }

        path = null;

        return false;
    }

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
