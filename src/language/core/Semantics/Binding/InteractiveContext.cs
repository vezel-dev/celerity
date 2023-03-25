namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed partial class InteractiveContext
{
    public static InteractiveContext Default { get; } =
        new(ImmutableDictionary<string, ModulePath>.Empty, ImmutableDictionary<string, InteractiveSymbol>.Empty);

    private readonly ImmutableDictionary<string, ModulePath> _uses;

    private readonly ImmutableDictionary<string, InteractiveSymbol> _symbols;

    private InteractiveContext(
        ImmutableDictionary<string, ModulePath> uses, ImmutableDictionary<string, InteractiveSymbol> symbols)
    {
        _uses = uses;
        _symbols = symbols;
    }

    public bool TryGetUse(string name, [MaybeNullWhen(false)] out ModulePath path)
    {
        Check.Argument(UseRegex().IsMatch(name), name);

        return _uses.TryGetValue(name, out path);
    }

    public InteractiveContext AddUse(string name, ModulePath path)
    {
        Check.Argument(UseRegex().IsMatch(name), name);
        Check.Null(path);

        return new(_uses.Add(name, path), _symbols);
    }

    public InteractiveContext RemoveUse(string name)
    {
        Check.Argument(UseRegex().IsMatch(name), name);

        return new(_uses.Remove(name), _symbols);
    }

    public bool TryGetSymbol(string name, [MaybeNullWhen(false)] out InteractiveSymbol symbol)
    {
        Check.Argument(SymbolRegex().IsMatch(name), name);

        return _symbols.TryGetValue(name, out symbol);
    }

    public InteractiveContext AddSymbol(string name, bool mutable)
    {
        Check.Argument(SymbolRegex().IsMatch(name), name);

        return new(_uses, _symbols.Add(name, new(name, mutable)));
    }

    public InteractiveContext RemoveSymbol(string name)
    {
        Check.Argument(SymbolRegex().IsMatch(name), name);

        return new(_uses, _symbols.Remove(name));
    }

    [GeneratedRegex(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex UseRegex();

    [GeneratedRegex(@"^[a-z][_0-9a-z]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex SymbolRegex();
}
