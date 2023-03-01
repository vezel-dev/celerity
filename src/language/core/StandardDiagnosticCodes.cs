using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language;

public static class StandardDiagnosticCodes
{
    // Codes are stable. New codes must be added at the end.

    public static SourceDiagnosticCode UnrecognizedCharacter { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteExclamationEquals { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteIntegerLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidIntegerLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteRealLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidRealLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteStringLiteral { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteEscapeSequence { get; } = CreateCode();

    public static SourceDiagnosticCode IncompleteUnicodeEscapeSequence { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidUnicodeEscapeSequence { get; } = CreateCode();

    // TODO: Create more specific diagnostics for certain kinds of missing tokens.
    public static SourceDiagnosticCode ExpectedToken { get; } = CreateCode();

    public static SourceDiagnosticCode MissingDeclaration { get; } = CreateCode();

    public static SourceDiagnosticCode MissingStatement { get; } = CreateCode();

    public static SourceDiagnosticCode MissingType { get; } = CreateCode();

    public static SourceDiagnosticCode MissingExpression { get; } = CreateCode();

    public static SourceDiagnosticCode MissingBinding { get; } = CreateCode();

    public static SourceDiagnosticCode MissingPattern { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateUseDeclaration { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateCodeDeclaration { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateParameterBinding { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateVariableBinding { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateAggregateExpressionField { get; } = CreateCode();

    public static SourceDiagnosticCode MissingEnclosingLambda { get; } = CreateCode();

    public static SourceDiagnosticCode MissingEnclosingLoop { get; } = CreateCode();

    public static SourceDiagnosticCode UnresolvedIdentifier { get; } = CreateCode();

    public static SourceDiagnosticCode IllegalTestReference { get; } = CreateCode();

    public static SourceDiagnosticCode InvalidAssignmentTarget { get; } = CreateCode();

    public static SourceDiagnosticCode ImmutableAssignmentTarget { get; } = CreateCode();

    public static SourceDiagnosticCode DuplicateAggregatePatternField { get; } = CreateCode();

    private static int _code;

    private static SourceDiagnosticCode CreateCode()
    {
        return SourceDiagnosticCode.CreateError(_code++);
    }
}
