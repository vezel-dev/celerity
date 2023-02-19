using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax;

public static class SyntaxFacts
{
    public static SyntaxTokenKind? GetKeywordKind(scoped ReadOnlySpan<char> text)
    {
        return GetNormalKeywordKind(text) ?? GetTypeKeywordKind(text) ?? GetReservedKeywordKind(text);
    }

    public static SyntaxTokenKind? GetNormalKeywordKind(scoped ReadOnlySpan<char> text)
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
            "type" => SyntaxTokenKind.TypeKeyword,
            "use" => SyntaxTokenKind.UseKeyword,
            "while" => SyntaxTokenKind.WhileKeyword,
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
            _ => null,
        };
    }

    public static SyntaxTokenKind? GetReservedKeywordKind(scoped ReadOnlySpan<char> text)
    {
        return text switch
        {
            "friend" => SyntaxTokenKind.FriendKeyword,
            "macro" => SyntaxTokenKind.MacroKeyword,
            "meta" => SyntaxTokenKind.MetaKeyword,
            "pragma" => SyntaxTokenKind.PragmaKeyword,
            "quote" => SyntaxTokenKind.QuoteKeyword,
            "try" => SyntaxTokenKind.TryKeyword,
            "unquote" => SyntaxTokenKind.UnquoteKeyword,
            "with" => SyntaxTokenKind.WithKeyword,
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

    public static bool IsKeyword(SyntaxTokenKind kind)
    {
        return IsNormalKeyword(kind) || IsTypeKeyword(kind) || IsReservedKeyword(kind);
    }

    public static bool IsNormalKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
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
            SyntaxTokenKind.TypeKeyword or
            SyntaxTokenKind.UseKeyword or
            SyntaxTokenKind.WhileKeyword => true,
            _ => false,
        };
    }

    public static bool IsTypeKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
            SyntaxTokenKind.AgentKeyword or
            SyntaxTokenKind.AnyKeyword or
            SyntaxTokenKind.AtomKeyword or
            SyntaxTokenKind.BoolKeyword or
            SyntaxTokenKind.HandleKeyword or
            SyntaxTokenKind.IntKeyword or
            SyntaxTokenKind.NoneKeyword or
            SyntaxTokenKind.RealKeyword or
            SyntaxTokenKind.RefKeyword or
            SyntaxTokenKind.StrKeyword => true,
            _ => false,
        };
    }

    public static bool IsReservedKeyword(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
            SyntaxTokenKind.FriendKeyword or
            SyntaxTokenKind.MacroKeyword or
            SyntaxTokenKind.MetaKeyword or
            SyntaxTokenKind.PragmaKeyword or
            SyntaxTokenKind.QuoteKeyword or
            SyntaxTokenKind.TryKeyword or
            SyntaxTokenKind.UnquoteKeyword or
            SyntaxTokenKind.WithKeyword or
            SyntaxTokenKind.YieldKeyword => true,
            _ => false,
        };
    }

    public static bool IsOperator(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
            SyntaxTokenKind.BitwiseOperator or
            SyntaxTokenKind.ShiftOperator or
            SyntaxTokenKind.MultiplicativeOperator or
            SyntaxTokenKind.AdditiveOperator => true,
            _ => false,
        };
    }

    public static bool IsIdentifier(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
            SyntaxTokenKind.UpperIdentifier or
            SyntaxTokenKind.LowerIdentifier or
            SyntaxTokenKind.DiscardIdentifier => true,
            _ => false,
        };
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

        return kind switch
        {
            SyntaxTokenKind.NilLiteral or
            SyntaxTokenKind.BooleanLiteral or
            SyntaxTokenKind.IntegerLiteral or
            SyntaxTokenKind.RealLiteral or
            SyntaxTokenKind.AtomLiteral or
            SyntaxTokenKind.StringLiteral => true,
            _ => false,
        };
    }

    public static bool IsDeclarationStarter(SyntaxTokenKind kind)
    {
        return kind switch
        {
            SyntaxTokenKind.ConstKeyword or
            SyntaxTokenKind.ExtKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.PubKeyword or
            SyntaxTokenKind.TestKeyword or
            SyntaxTokenKind.TypeKeyword or
            SyntaxTokenKind.UseKeyword => true,
            _ => false,
        };
    }

    public static bool IsStatementStarter(SyntaxTokenKind kind)
    {
        return kind switch
        {
            SyntaxTokenKind.AssertKeyword or
            SyntaxTokenKind.DeferKeyword or
            SyntaxTokenKind.LetKeyword or
            SyntaxTokenKind.UseKeyword => true,
            _ => IsExpressionStarter(kind),
        };
    }

    public static bool IsExpressionStarter(SyntaxTokenKind kind)
    {
        return kind switch
        {
            SyntaxTokenKind.BitwiseOperator or
            SyntaxTokenKind.ShiftOperator or
            SyntaxTokenKind.MultiplicativeOperator or
            SyntaxTokenKind.AdditiveOperator or
            SyntaxTokenKind.Hash or
            SyntaxTokenKind.OpenParen or
            SyntaxTokenKind.OpenBracket or
            SyntaxTokenKind.OpenBrace or
            SyntaxTokenKind.BreakKeyword or
            SyntaxTokenKind.CondKeyword or
            SyntaxTokenKind.ErrKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.ForKeyword or
            SyntaxTokenKind.IfKeyword or
            SyntaxTokenKind.MatchKeyword or
            SyntaxTokenKind.MutKeyword or
            SyntaxTokenKind.NextKeyword or
            SyntaxTokenKind.NotKeyword or
            SyntaxTokenKind.RaiseKeyword or
            SyntaxTokenKind.RecKeyword or
            SyntaxTokenKind.RecvKeyword or
            SyntaxTokenKind.RetKeyword or
            SyntaxTokenKind.TailKeyword or
            SyntaxTokenKind.WhileKeyword or
            SyntaxTokenKind.UpperIdentifier or
            SyntaxTokenKind.LowerIdentifier or
            SyntaxTokenKind.DiscardIdentifier or
            SyntaxTokenKind.NilLiteral or
            SyntaxTokenKind.BooleanLiteral or
            SyntaxTokenKind.IntegerLiteral or
            SyntaxTokenKind.RealLiteral or
            SyntaxTokenKind.AtomLiteral or
            SyntaxTokenKind.StringLiteral => true,
            _ => false,
        };
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
            SyntaxTokenKind.TypeKeyword => "'type' keyword",
            SyntaxTokenKind.UseKeyword => "'use' keyword",
            SyntaxTokenKind.WhileKeyword => "'while' keyword",
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
