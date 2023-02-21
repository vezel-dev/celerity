namespace Vezel.Celerity.Syntax;

public static class SyntaxDiagnosticCodes
{
    // Codes are stable. New codes must be added at the end.

    public static SourceDiagnosticCode UnrecognizedCharacter { get; } = CreateError();

    public static SourceDiagnosticCode IncompleteExclamationEquals { get; } = CreateError();

    public static SourceDiagnosticCode LowercaseBaseIndicator { get; } = CreateWarning();

    public static SourceDiagnosticCode IncompleteIntegerLiteral { get; } = CreateError();

    public static SourceDiagnosticCode InvalidIntegerLiteral { get; } = CreateError();

    public static SourceDiagnosticCode IncompleteRealLiteral { get; } = CreateError();

    public static SourceDiagnosticCode InvalidRealLiteral { get; } = CreateError();

    public static SourceDiagnosticCode UnclosedStringLiteral { get; } = CreateError();

    public static SourceDiagnosticCode IncompleteEscapeSequence { get; } = CreateError();

    public static SourceDiagnosticCode IncompleteUnicodeEscapeSequence { get; } = CreateError();

    public static SourceDiagnosticCode InvalidUnicodeEscapeSequence { get; } = CreateError();

    // TODO: Create more specific diagnostics for certain kinds of missing characters.
    public static SourceDiagnosticCode ExpectedToken { get; } = CreateError();

    public static SourceDiagnosticCode MissingDeclaration { get; } = CreateError();

    public static SourceDiagnosticCode MissingStatement { get; } = CreateError();

    public static SourceDiagnosticCode MissingType { get; } = CreateError();

    public static SourceDiagnosticCode MissingExpression { get; } = CreateError();

    public static SourceDiagnosticCode MissingPattern { get; } = CreateError();

    public static SourceDiagnosticCode MissingPatternBinding { get; } = CreateError();

    private static int _code;

    private static SourceDiagnosticCode CreateWarning()
    {
        return SourceDiagnosticCode.CreateError(++_code);
    }

    private static SourceDiagnosticCode CreateError()
    {
        return SourceDiagnosticCode.CreateError(++_code);
    }
}
