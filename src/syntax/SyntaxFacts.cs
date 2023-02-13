using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax;

public static class SyntaxFacts
{
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

    public static bool IsKeyword(SyntaxTokenKind kind)
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
            SyntaxTokenKind.FalseKeyword or
            SyntaxTokenKind.FnKeyword or
            SyntaxTokenKind.IfKeyword or
            SyntaxTokenKind.InKeyword or
            SyntaxTokenKind.LetKeyword or
            SyntaxTokenKind.MatchKeyword or
            SyntaxTokenKind.ModKeyword or
            SyntaxTokenKind.MutKeyword or
            SyntaxTokenKind.NextKeyword or
            SyntaxTokenKind.NilKeyword or
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
            SyntaxTokenKind.TrueKeyword or
            SyntaxTokenKind.TypeKeyword or
            SyntaxTokenKind.UseKeyword or
            SyntaxTokenKind.WhileKeyword or
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

    public static bool IsLiteral(SyntaxTokenKind kind)
    {
        Check.Enum(kind);

        return kind switch
        {
            SyntaxTokenKind.IntegerLiteral or
            SyntaxTokenKind.RealLiteral or
            SyntaxTokenKind.AtomLiteral or
            SyntaxTokenKind.StringLiteral => true,
            _ => false,
        };
    }
}
