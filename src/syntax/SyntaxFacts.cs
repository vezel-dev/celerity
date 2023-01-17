namespace Vezel.Celerity.Syntax;

public static class SyntaxFacts
{
    public static SyntaxTokenKind? GetNormalKeywordKind(string text)
    {
        Check.Null(text);

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
            "false" => SyntaxTokenKind.FalseKeyword,
            "fn" => SyntaxTokenKind.FnKeyword,
            "if" => SyntaxTokenKind.IfKeyword,
            "in" => SyntaxTokenKind.InKeyword,
            "let" => SyntaxTokenKind.LetKeyword,
            "match" => SyntaxTokenKind.MatchKeyword,
            "mod" => SyntaxTokenKind.ModKeyword,
            "mut" => SyntaxTokenKind.MutKeyword,
            "next" => SyntaxTokenKind.NextKeyword,
            "nil" => SyntaxTokenKind.NilKeyword,
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
            "true" => SyntaxTokenKind.TrueKeyword,
            "type" => SyntaxTokenKind.TypeKeyword,
            "use" => SyntaxTokenKind.UseKeyword,
            "while" => SyntaxTokenKind.WhileKeyword,
            _ => null,
        };
    }

    public static SyntaxTokenKind? GetTypeKeywordKind(string text)
    {
        Check.Null(text);

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

    public static SyntaxTokenKind? GetReservedKeywordKind(string text)
    {
        Check.Null(text);

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
}
