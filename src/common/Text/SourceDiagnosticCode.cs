using Vezel.Celerity.Diagnostics;

namespace Vezel.Celerity.Text;

public readonly partial struct SourceDiagnosticCode :
    IEquatable<SourceDiagnosticCode>, IEqualityOperators<SourceDiagnosticCode, SourceDiagnosticCode, bool>
{
    public static SourceDiagnosticCode InternalError { get; } = CreateError(0000);

    public string Code { get; }

    public bool IsCustom => !char.IsAsciiLetterUpper(Code[0]);

    private SourceDiagnosticCode(string code)
    {
        Code = code;
    }

    internal static SourceDiagnosticCode CreateSuggestion(int code)
    {
        return new($"S{code}");
    }

    internal static SourceDiagnosticCode CreateWarning(int code)
    {
        return new($"W{code}");
    }

    internal static SourceDiagnosticCode CreateError(int code)
    {
        return new($"E{code}");
    }

    public static SourceDiagnosticCode Create(string code)
    {
        Check.Null(code);
        Check.Argument(CodeRegex().IsMatch(code), code);

        return new(code);
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
