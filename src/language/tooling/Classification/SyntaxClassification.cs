// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Classification;

public enum SyntaxClassification
{
    WhiteSpace,
    ShebangLine,
    Comment,
    Operator,
    Punctuator,
    DeclarationKeyword,
    OperationKeyword,
    TypeKeyword,
    [SuppressMessage("", "CA1700")]
    ReservedKeyword,
    NilLiteral,
    BooleanLiteral,
    IntegerLiteral,
    RealLiteral,
    AtomLiteral,
    StringLiteral,
    AttributeName,
    ModuleName,
    CodeParameterName,
    TypeName,
    TypeParameterName,
    UnresolvedName,
    ConstantName,
    FunctionName,
    TestName,
    VariableName,
    InteractiveName,
    ErrorName,
    FieldName,
    MessageName,
}
