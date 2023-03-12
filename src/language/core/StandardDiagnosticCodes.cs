using Vezel.Celerity.Language.Diagnostics;

namespace Vezel.Celerity.Language;

public static class StandardDiagnosticCodes
{
    // Codes are stable. New codes must be added at the end.

    public static DiagnosticCode InternalError { get; } = CreateCode();

    public static DiagnosticCode UnrecognizedCharacter { get; } = CreateCode();

    public static DiagnosticCode UnsupportedWhiteSpaceCharacter { get; } = CreateCode();

    public static DiagnosticCode UnsupportedNewLineCharacter { get; } = CreateCode();

    public static DiagnosticCode IncompleteExclamationEquals { get; } = CreateCode();

    public static DiagnosticCode IncompleteIntegerLiteral { get; } = CreateCode();

    public static DiagnosticCode InvalidIntegerLiteral { get; } = CreateCode();

    public static DiagnosticCode IncompleteRealLiteral { get; } = CreateCode();

    public static DiagnosticCode InvalidRealLiteral { get; } = CreateCode();

    public static DiagnosticCode IncompleteStringLiteral { get; } = CreateCode();

    public static DiagnosticCode IncompleteEscapeSequence { get; } = CreateCode();

    public static DiagnosticCode IncompleteUnicodeEscapeSequence { get; } = CreateCode();

    public static DiagnosticCode InvalidUnicodeEscapeSequence { get; } = CreateCode();

    public static DiagnosticCode ExpectedToken { get; } = CreateCode();

    public static DiagnosticCode MissingDeclaration { get; } = CreateCode();

    public static DiagnosticCode MissingStatement { get; } = CreateCode();

    public static DiagnosticCode MissingType { get; } = CreateCode();

    public static DiagnosticCode MissingExpression { get; } = CreateCode();

    public static DiagnosticCode MissingBinding { get; } = CreateCode();

    public static DiagnosticCode MissingPattern { get; } = CreateCode();

    public static DiagnosticCode DuplicateUseDeclaration { get; } = CreateCode();

    public static DiagnosticCode DuplicateCodeDeclaration { get; } = CreateCode();

    public static DiagnosticCode DuplicateParameterBinding { get; } = CreateCode();

    public static DiagnosticCode DuplicateVariableBinding { get; } = CreateCode();

    public static DiagnosticCode DuplicateAggregateExpressionField { get; } = CreateCode();

    public static DiagnosticCode MissingEnclosingLambda { get; } = CreateCode();

    public static DiagnosticCode ErrorInInfallibleContext { get; } = CreateCode();

    public static DiagnosticCode MissingEnclosingLoop { get; } = CreateCode();

    public static DiagnosticCode UnresolvedIdentifier { get; } = CreateCode();

    public static DiagnosticCode IllegalTestReference { get; } = CreateCode();

    public static DiagnosticCode InvalidAssignmentTarget { get; } = CreateCode();

    public static DiagnosticCode ImmutableAssignmentTarget { get; } = CreateCode();

    public static DiagnosticCode DuplicateAggregatePatternField { get; } = CreateCode();

    private static int _code;

    private static DiagnosticCode CreateCode()
    {
        return DiagnosticCode.CreateError(_code++);
    }
}
