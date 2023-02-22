namespace Vezel.Celerity.Syntax;

public static class SyntaxDiagnosticCodes
{
    // Codes are stable. New codes must be added at the end.

    public static SourceDiagnosticCode UnrecognizedCharacter { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteExclamationEquals { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteIntegerLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidIntegerLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteRealLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidRealLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode UnclosedStringLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteEscapeSequence { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteUnicodeEscapeSequence { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidUnicodeEscapeSequence { get; } = CreateCode();

    // TODO: Create more specific diagnostics for certain kinds of missing characters.
    public static SourceDiagnosticCode ExpectedToken { get; } = CreateCode();

    public static SourceDiagnosticCode MissingDeclaration { get; } = CreateCode();

    public static SourceDiagnosticCode MissingStatement { get; } = CreateCode();

    public static SourceDiagnosticCode MissingType { get; } = CreateCode();

    public static SourceDiagnosticCode MissingExpression { get; } = CreateCode();

    public static SourceDiagnosticCode MissingPattern { get; } = CreateCode();

    public static SourceDiagnosticCode MissingPatternBinding { get; } = CreateCode();

    private static int _code = 1000;

    private static SourceDiagnosticCode CreateCode()
    {
        return SourceDiagnosticCode.CreateError(_code++);
    }
}
