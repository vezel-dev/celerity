namespace Vezel.Celerity.Syntax;

public enum SyntaxTokenKind
{
    Missing,
    EndOfInput,
    Unrecognized,
    BitwiseOperator,
    ShiftOperator,
    MultiplicativeOperator,
    AdditiveOperator,
    OpenAngleMinus,
    Equals,
    EqualsEquals,
    ExclamationEquals,
    CloseAngle,
    CloseAngleEquals,
    OpenAngle,
    OpenAngleEquals,
    Dot,
    DotDot,
    Comma,
    Colon,
    ColonColon,
    Semicolon,
    MinusCloseAngle,
    EqualsCloseAngle,
    At,
    Hash,
    Question,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    OpenBrace,
    CloseBrace,
    AndKeyword,
    AsKeyword,
    AssertKeyword,
    BreakKeyword,
    CatchKeyword,
    CondKeyword,
    ConstKeyword,
    DeferKeyword,
    ElseKeyword,
    ErrKeyword,
    ExtKeyword,
    FalseKeyword,
    FnKeyword,
    IfKeyword,
    InKeyword,
    LetKeyword,
    MatchKeyword,
    ModKeyword,
    MutKeyword,
    NextKeyword,
    NilKeyword,
    NotKeyword,
    OpaqueKeyword,
    OrKeyword,
    PubKeyword,
    RaiseKeyword,
    RecKeyword,
    RecvKeyword,
    RetKeyword,
    TailKeyword,
    TestKeyword,
    TrueKeyword,
    TypeKeyword,
    UseKeyword,
    WhileKeyword,
    AgentKeyword,
    AnyKeyword,
    AtomKeyword,
    BoolKeyword,
    HandleKeyword,
    IntKeyword,
    NoneKeyword,
    RealKeyword,
    RefKeyword,
    StrKeyword,
    FriendKeyword,
    MacroKeyword,
    MetaKeyword,
    PragmaKeyword,
    QuoteKeyword,
    TryKeyword,
    UnquoteKeyword,
    WithKeyword,
    YieldKeyword,
    UpperIdentifier,
    LowerIdentifier,
    DiscardIdentifier,
    IntegerLiteral,
    RealLiteral,
    AtomLiteral,
    StringLiteral,
}