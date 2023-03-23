namespace Vezel.Celerity.Language.Diagnostics;

public readonly partial struct DiagnosticCode :
    IEquatable<DiagnosticCode>, IEqualityOperators<DiagnosticCode, DiagnosticCode, bool>
{
    public string Code { get; }

    public bool IsStandard => char.IsAsciiLetterUpper(Code[0]);

    private DiagnosticCode(string code)
    {
        Code = code;
    }

    internal static DiagnosticCode CreateStandard(int code)
    {
        return new($"E{code:0000}");
    }

    public static DiagnosticCode Create(string name)
    {
        Check.Argument(TryCreate(name, out var code), name);

        return code;
    }

    public static bool TryCreate(string name, out DiagnosticCode code)
    {
        Check.Null(name);

        if (CodeRegex().IsMatch(name))
        {
            code = new(name);

            return true;
        }

        code = default;

        return false;
    }

    public static bool operator ==(DiagnosticCode left, DiagnosticCode right) => left.Equals(right);

    public static bool operator !=(DiagnosticCode left, DiagnosticCode right) => !left.Equals(right);

    public bool Equals(DiagnosticCode other)
    {
        return Code == other.Code;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DiagnosticCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code);
    }

    public override string ToString()
    {
        return Code;
    }

    [GeneratedRegex(@"^[a-z]+(-[a-z]+)*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();
}
