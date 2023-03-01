namespace Vezel.Celerity.Language.Text;

public readonly partial struct SourceDiagnosticCode :
    IEquatable<SourceDiagnosticCode>, IEqualityOperators<SourceDiagnosticCode, SourceDiagnosticCode, bool>
{
    public static SourceDiagnosticCode InternalError { get; } = CreateError(0000);

    public string Code { get; }

    public bool IsStandard => char.IsAsciiLetterUpper(Code[0]);

    private SourceDiagnosticCode(string code)
    {
        Code = code;
    }

    internal static SourceDiagnosticCode CreateSuggestion(int code)
    {
        return new($"S{code:0000}");
    }

    internal static SourceDiagnosticCode CreateWarning(int code)
    {
        return new($"W{code:0000}");
    }

    internal static SourceDiagnosticCode CreateError(int code)
    {
        return new($"E{code:0000}");
    }

    public static SourceDiagnosticCode Create(string name)
    {
        Check.Argument(TryCreate(name, out var code), name);

        return code;
    }

    public static bool TryCreate(string name, out SourceDiagnosticCode code)
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

    public static bool operator ==(SourceDiagnosticCode left, SourceDiagnosticCode right) => left.Equals(right);

    public static bool operator !=(SourceDiagnosticCode left, SourceDiagnosticCode right) => !left.Equals(right);

    public bool Equals(SourceDiagnosticCode other)
    {
        return Code == other.Code;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceDiagnosticCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code);
    }

    public override string ToString()
    {
        return Code;
    }

    [GeneratedRegex(@"^[a-z]+(-[a-z]+)*?$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();
}