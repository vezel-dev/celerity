// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Syntax;

public static partial class SyntaxFacts
{
    public static SyntaxTokenKind? GetRegularKeywordKind(scoped ReadOnlySpan<char> text)
    {
        return text switch
        {
            "and" => SyntaxTokenKind.AndKeyword,
            "as" => SyntaxTokenKind.AsKeyword,
            "assert" => SyntaxTokenKind.AssertKeyword,
            "break" => SyntaxTokenKind.BreakKeyword,
            "catch" => SyntaxTokenKind.CatchKeyword,
            "cond" => SyntaxTokenKind.CondKeyword,
            "const" => SyntaxTokenKind.ConstKeyword,
            "defer" => SyntaxTokenKind.DeferKeyword,
            "else" => SyntaxTokenKind.ElseKeyword,
            "err" => SyntaxTokenKind.ErrKeyword,
            "ext" => SyntaxTokenKind.ExtKeyword,
            "fn" => SyntaxTokenKind.FnKeyword,
            "for" => SyntaxTokenKind.ForKeyword,
            "if" => SyntaxTokenKind.IfKeyword,
            "in" => SyntaxTokenKind.InKeyword,
            "let" => SyntaxTokenKind.LetKeyword,
            "match" => SyntaxTokenKind.MatchKeyword,
            "meta" => SyntaxTokenKind.MetaKeyword,
            "mod" => SyntaxTokenKind.ModKeyword,
            "mut" => SyntaxTokenKind.MutKeyword,
            "next" => SyntaxTokenKind.NextKeyword,
            "not" => SyntaxTokenKind.NotKeyword,
            "opaque" => SyntaxTokenKind.OpaqueKeyword,
            "or" => SyntaxTokenKind.OrKeyword,
            "pub" => SyntaxTokenKind.PubKeyword,
            "raise" => SyntaxTokenKind.RaiseKeyword,
            "rec" => SyntaxTokenKind.RecKeyword,
            "recv" => SyntaxTokenKind.RecvKeyword,
            "ret" => SyntaxTokenKind.RetKeyword,
            "tail" => SyntaxTokenKind.TailKeyword,
            "test" => SyntaxTokenKind.TestKeyword,
            "this" => SyntaxTokenKind.ThisKeyword,
            "try" => SyntaxTokenKind.TryKeyword,
            "type" => SyntaxTokenKind.TypeKeyword,
            "use" => SyntaxTokenKind.UseKeyword,
            "while" => SyntaxTokenKind.WhileKeyword,
            "with" => SyntaxTokenKind.WithKeyword,
            _ => null,
        };
    }

    public static SyntaxTokenKind? GetTypeKeywordKind(scoped ReadOnlySpan<char> text)
    {
        return text switch
        {
            "agent" => SyntaxTokenKind.AgentKeyword,
            "any" => SyntaxTokenKind.AnyKeyword,
            "atom" => SyntaxTokenKind.AtomKeyword,
            "bool" => SyntaxTokenKind.BoolKeyword,
            "handle" => SyntaxTokenKind.HandleKeyword,
            "int" => SyntaxTokenKind.IntKeyword,
            "none" => SyntaxTokenKind.NoneKeyword,
            "real" => SyntaxTokenKind.RealKeyword,
            "ref" => SyntaxTokenKind.RefKeyword,
            "str" => SyntaxTokenKind.StrKeyword,
            "unk" => SyntaxTokenKind.UnkKeyword,
            _ => null,
        };
    }

    public static SyntaxTokenKind? GetReservedKeywordKind(scoped ReadOnlySpan<char> text)
    {
        return text switch
        {
            "friend" => SyntaxTokenKind.FriendKeyword,
            "macro" => SyntaxTokenKind.MacroKeyword,
            "quote" => SyntaxTokenKind.QuoteKeyword,
            "unquote" => SyntaxTokenKind.UnquoteKeyword,
            "yield" => SyntaxTokenKind.YieldKeyword,
            _ => null,
        };
    }

    public static SyntaxTokenKind? GetKeywordLiteralKind(scoped ReadOnlySpan<char> text)
    {
        return text switch
        {
            "false" or "true" => SyntaxTokenKind.BooleanLiteral,
            "nil" => SyntaxTokenKind.NilLiteral,
            _ => null,
        };
    }

    public static bool IsBindingIdentifier(scoped ReadOnlySpan<char> text)
    {
        return IsDiscardIdentifier(text) || IsCodeIdentifier(text);
    }

    public static bool IsCodeIdentifier(scoped ReadOnlySpan<char> text)
    {
        return IsLowerIdentifier(text) &&
               (GetRegularKeywordKind(text),
                GetReservedKeywordKind(text),
                GetKeywordLiteralKind(text)) == (null, null, null);
    }

    public static bool IsUpperIdentifier(scoped ReadOnlySpan<char> text)
    {
        return UpperIdentifierRegex().IsMatch(text);
    }

    [GeneratedRegex(@"^[A-Z][0-9a-zA-Z]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex UpperIdentifierRegex();

    public static bool IsLowerIdentifier(scoped ReadOnlySpan<char> text)
    {
        return LowerIdentifierRegex().IsMatch(text);
    }

    [GeneratedRegex(@"^[a-z][_0-9a-z]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex LowerIdentifierRegex();

    public static bool IsDiscardIdentifier(scoped ReadOnlySpan<char> text)
    {
        return DiscardIdentifierRegex().IsMatch(text);
    }

    [GeneratedRegex(@"^_[_0-9a-z]*$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex DiscardIdentifierRegex();

    public static bool IsRegularKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.AndKeyword or
            SyntaxTokenKind.AsKeyword or
            SyntaxTokenKind.AssertKeyword or
            SyntaxTokenKind.BreakKeyword or
            SyntaxTokenKind.CatchKeyword or
            SyntaxTokenKind.CondKeyword or
            SyntaxTokenKind.ConstKeyword or
            SyntaxTokenKind.DeferKeyword or
            SyntaxTokenKind.ElseKeyword or
            SyntaxTokenKind.ErrKeyword or
            SyntaxTokenKind.ExtKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.ForKeyword or
            SyntaxTokenKind.IfKeyword or
            SyntaxTokenKind.InKeyword or
            SyntaxTokenKind.LetKeyword or
            SyntaxTokenKind.MatchKeyword or
            SyntaxTokenKind.MetaKeyword or
            SyntaxTokenKind.ModKeyword or
            SyntaxTokenKind.MutKeyword or
            SyntaxTokenKind.NextKeyword or
            SyntaxTokenKind.NotKeyword or
            SyntaxTokenKind.OpaqueKeyword or
            SyntaxTokenKind.OrKeyword or
            SyntaxTokenKind.PubKeyword or
            SyntaxTokenKind.RaiseKeyword or
            SyntaxTokenKind.RecKeyword or
            SyntaxTokenKind.RecvKeyword or
            SyntaxTokenKind.RetKeyword or
            SyntaxTokenKind.TailKeyword or
            SyntaxTokenKind.TestKeyword or
            SyntaxTokenKind.ThisKeyword or
            SyntaxTokenKind.TryKeyword or
            SyntaxTokenKind.TypeKeyword or
            SyntaxTokenKind.UseKeyword or
            SyntaxTokenKind.WhileKeyword or
            SyntaxTokenKind.WithKeyword;
    }

    public static bool IsTypeKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.AgentKeyword or
            SyntaxTokenKind.AnyKeyword or
            SyntaxTokenKind.AtomKeyword or
            SyntaxTokenKind.BoolKeyword or
            SyntaxTokenKind.HandleKeyword or
            SyntaxTokenKind.IntKeyword or
            SyntaxTokenKind.NoneKeyword or
            SyntaxTokenKind.RealKeyword or
            SyntaxTokenKind.RefKeyword or
            SyntaxTokenKind.StrKeyword or
            SyntaxTokenKind.UnkKeyword;
    }

    public static bool IsReservedKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.FriendKeyword or
            SyntaxTokenKind.MacroKeyword or
            SyntaxTokenKind.QuoteKeyword or
            SyntaxTokenKind.UnquoteKeyword or
            SyntaxTokenKind.YieldKeyword;
    }

    public static bool IsLiteralKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.NilLiteral or
            SyntaxTokenKind.BooleanLiteral;
    }

    public static bool IsCustomOperator(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.BitwiseOperator or
            SyntaxTokenKind.ShiftOperator or
            SyntaxTokenKind.MultiplicativeOperator or
            SyntaxTokenKind.AdditiveOperator;
    }

    public static bool IsBindingIdentifier(SyntaxTokenKind kind)
    {
        return kind == SyntaxTokenKind.DiscardIdentifier || IsCodeIdentifier(kind);
    }

    public static bool IsCodeIdentifier(SyntaxTokenKind kind)
    {
        return kind == SyntaxTokenKind.LowerIdentifier || IsTypeKeyword(kind);
    }

    public static bool IsLiteral(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.NilLiteral or
            SyntaxTokenKind.BooleanLiteral or
            SyntaxTokenKind.IntegerLiteral or
            SyntaxTokenKind.RealLiteral or
            SyntaxTokenKind.AtomLiteral or
            SyntaxTokenKind.StringLiteral;
    }

    public static bool IsSubmissionStarter(SyntaxTokenKind kind)
    {
        return IsDeclarationSubmissionStarter(kind) || IsStatementSubmissionStarter(kind);
    }

    public static bool IsDeclarationSubmissionStarter(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind == SyntaxTokenKind.UseKeyword;
    }

    public static bool IsStatementSubmissionStarter(SyntaxTokenKind kind)
    {
        return kind is
            SyntaxTokenKind.AssertKeyword or
            SyntaxTokenKind.LetKeyword ||
            IsExpressionStarter(kind);
    }

    public static bool IsDeclarationStarter(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind is
            SyntaxTokenKind.ConstKeyword or
            SyntaxTokenKind.ErrKeyword or
            SyntaxTokenKind.ExtKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.PubKeyword or
            SyntaxTokenKind.TestKeyword or
            SyntaxTokenKind.TypeKeyword ||
            IsDeclarationSubmissionStarter(kind);
    }

    public static bool IsStatementStarter(SyntaxTokenKind kind)
    {
        return kind == SyntaxTokenKind.DeferKeyword || IsStatementSubmissionStarter(kind);
    }

    internal static bool IsFlowExpressionStarter(SyntaxTokenKind kind)
    {
        return kind is
            SyntaxTokenKind.OpenBrace or
            SyntaxTokenKind.CondKeyword or
            SyntaxTokenKind.ForKeyword or
            SyntaxTokenKind.IfKeyword or
            SyntaxTokenKind.MatchKeyword or
            SyntaxTokenKind.RecvKeyword or
            SyntaxTokenKind.TryKeyword or
            SyntaxTokenKind.WhileKeyword;
    }

    public static bool IsExpressionStarter(SyntaxTokenKind kind)
    {
        return kind is
            SyntaxTokenKind.Hash or
            SyntaxTokenKind.OpenParen or
            SyntaxTokenKind.OpenBracket or
            SyntaxTokenKind.BreakKeyword or
            SyntaxTokenKind.ErrKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.MutKeyword or
            SyntaxTokenKind.NextKeyword or
            SyntaxTokenKind.NotKeyword or
            SyntaxTokenKind.RaiseKeyword or
            SyntaxTokenKind.RecKeyword or
            SyntaxTokenKind.RetKeyword or
            SyntaxTokenKind.TailKeyword or
            SyntaxTokenKind.ThisKeyword or
            SyntaxTokenKind.UpperIdentifier ||
            IsFlowExpressionStarter(kind) ||
            IsCustomOperator(kind) ||
            IsBindingIdentifier(kind) ||
            IsLiteral(kind);
    }

    internal static bool IsInternable(SyntaxTriviaKind kind)
    {
        return kind == SyntaxTriviaKind.NewLine;
    }

    internal static bool IsInternable(SyntaxTokenKind kind)
    {
        return kind is
            SyntaxTokenKind.Equals or
            SyntaxTokenKind.EqualsEquals or
            SyntaxTokenKind.ExclamationEquals or
            SyntaxTokenKind.CloseAngle or
            SyntaxTokenKind.CloseAngleEquals or
            SyntaxTokenKind.OpenAngle or
            SyntaxTokenKind.OpenAngleEquals or
            SyntaxTokenKind.Dot or
            SyntaxTokenKind.DotDot or
            SyntaxTokenKind.Comma or
            SyntaxTokenKind.Colon or
            SyntaxTokenKind.ColonColon or
            SyntaxTokenKind.Semicolon or
            SyntaxTokenKind.MinusCloseAngle or
            SyntaxTokenKind.At or
            SyntaxTokenKind.Hash or
            SyntaxTokenKind.Question or
            SyntaxTokenKind.OpenParen or
            SyntaxTokenKind.CloseParen or
            SyntaxTokenKind.OpenBracket or
            SyntaxTokenKind.CloseBracket or
            SyntaxTokenKind.OpenBrace or
            SyntaxTokenKind.CloseBrace ||
            IsRegularKeyword(kind) ||
            IsTypeKeyword(kind) ||
            IsReservedKeyword(kind) ||
            IsLiteralKeyword(kind);
    }

    internal static string GetFriendlyName(SyntaxTokenKind kind)
    {
        return kind switch
        {
            SyntaxTokenKind.BitwiseOperator => "bitwise operator",
            SyntaxTokenKind.ShiftOperator => "shift operator",
            SyntaxTokenKind.MultiplicativeOperator => "multiplicative operator",
            SyntaxTokenKind.AdditiveOperator => "additive operator",
            SyntaxTokenKind.Equals => "'='",
            SyntaxTokenKind.EqualsEquals => "'=='",
            SyntaxTokenKind.ExclamationEquals => "'!='",
            SyntaxTokenKind.CloseAngle => "'>'",
            SyntaxTokenKind.CloseAngleEquals => "'>='",
            SyntaxTokenKind.OpenAngle => "'<'",
            SyntaxTokenKind.OpenAngleEquals => "'<='",
            SyntaxTokenKind.Dot => "'.'",
            SyntaxTokenKind.DotDot => "'..'",
            SyntaxTokenKind.Comma => "','",
            SyntaxTokenKind.Colon => "':'",
            SyntaxTokenKind.ColonColon => "'::'",
            SyntaxTokenKind.Semicolon => "';'",
            SyntaxTokenKind.MinusCloseAngle => "'->'",
            SyntaxTokenKind.At => "'@'",
            SyntaxTokenKind.Hash => "'#'",
            SyntaxTokenKind.Question => "'?'",
            SyntaxTokenKind.OpenParen => "'('",
            SyntaxTokenKind.CloseParen => "')'",
            SyntaxTokenKind.OpenBracket => "'['",
            SyntaxTokenKind.CloseBracket => "']'",
            SyntaxTokenKind.OpenBrace => "'{'",
            SyntaxTokenKind.CloseBrace => "'}'",
            SyntaxTokenKind.AndKeyword => "'and' keyword",
            SyntaxTokenKind.AsKeyword => "'as' keyword",
            SyntaxTokenKind.AssertKeyword => "'assert' keyword",
            SyntaxTokenKind.BreakKeyword => "'break' keyword",
            SyntaxTokenKind.CatchKeyword => "'catch' keyword",
            SyntaxTokenKind.CondKeyword => "'cond' keyword",
            SyntaxTokenKind.ConstKeyword => "'const' keyword",
            SyntaxTokenKind.DeferKeyword => "'defer' keyword",
            SyntaxTokenKind.ElseKeyword => "'else' keyword",
            SyntaxTokenKind.ErrKeyword => "'err' keyword",
            SyntaxTokenKind.ExtKeyword => "'ext' keyword",
            SyntaxTokenKind.FnKeyword => "'fn' keyword",
            SyntaxTokenKind.ForKeyword => "'for' keyword",
            SyntaxTokenKind.IfKeyword => "'if' keyword",
            SyntaxTokenKind.InKeyword => "'in' keyword",
            SyntaxTokenKind.LetKeyword => "'let' keyword",
            SyntaxTokenKind.MatchKeyword => "'match' keyword",
            SyntaxTokenKind.ModKeyword => "'mod' keyword",
            SyntaxTokenKind.MutKeyword => "'mut' keyword",
            SyntaxTokenKind.NextKeyword => "'next' keyword",
            SyntaxTokenKind.NotKeyword => "'not' keyword",
            SyntaxTokenKind.OpaqueKeyword => "'opaque' keyword",
            SyntaxTokenKind.OrKeyword => "'or' keyword",
            SyntaxTokenKind.PubKeyword => "'pub' keyword",
            SyntaxTokenKind.RaiseKeyword => "'raise' keyword",
            SyntaxTokenKind.RecKeyword => "'rec' keyword",
            SyntaxTokenKind.RecvKeyword => "'recv' keyword",
            SyntaxTokenKind.RetKeyword => "'ret' keyword",
            SyntaxTokenKind.TailKeyword => "'tail' keyword",
            SyntaxTokenKind.TestKeyword => "'test' keyword",
            SyntaxTokenKind.ThisKeyword => "'this' keyword",
            SyntaxTokenKind.TryKeyword => "'try' keyword",
            SyntaxTokenKind.TypeKeyword => "'type' keyword",
            SyntaxTokenKind.UseKeyword => "'use' keyword",
            SyntaxTokenKind.WhileKeyword => "'while' keyword",
            SyntaxTokenKind.WithKeyword => "'with' keyword",
            SyntaxTokenKind.AgentKeyword => "'agent' keyword",
            SyntaxTokenKind.AnyKeyword => "'any' keyword",
            SyntaxTokenKind.AtomKeyword => "'atom' keyword",
            SyntaxTokenKind.BoolKeyword => "'bool' keyword",
            SyntaxTokenKind.HandleKeyword => "'handle' keyword",
            SyntaxTokenKind.IntKeyword => "'int' keyword",
            SyntaxTokenKind.NoneKeyword => "'none' keyword",
            SyntaxTokenKind.RealKeyword => "'real' keyword",
            SyntaxTokenKind.RefKeyword => "'ref' keyword",
            SyntaxTokenKind.StrKeyword => "'str' keyword",
            SyntaxTokenKind.UpperIdentifier => "uppercase identifier",
            SyntaxTokenKind.LowerIdentifier => "lowercase identifier",
            SyntaxTokenKind.DiscardIdentifier => "discard identifier",
            SyntaxTokenKind.NilLiteral => "'nil' literal",
            SyntaxTokenKind.BooleanLiteral => "'true' or 'false' literal",
            SyntaxTokenKind.IntegerLiteral => "integer literal",
            SyntaxTokenKind.RealLiteral => "real literal",
            SyntaxTokenKind.AtomLiteral => "atom literal",
            SyntaxTokenKind.StringLiteral => "string literal",
            _ => throw new UnreachableException(),
        };
    }
}
