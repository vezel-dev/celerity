using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

internal sealed class LanguageLexer
{
    private const int UnicodeEscapeSequenceLength = 6;

    private readonly SourceText _text;

    private readonly ListReader<char> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly StringBuilder _chars = new();

    private readonly StringBuilder _trivia = new();

    private readonly StringBuilder _string = new();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _leading = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _trailing = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private int _position;

    private bool _errors;

    public LanguageLexer(SourceText text, SyntaxMode mode, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _text = text;
        _reader = new(text);
        _mode = mode;
        _diagnostics = diagnostics;
    }

    private char? Peek1()
    {
        return _reader.TryPeek(0, out var ch) ? ch : null;
    }

    private (char? First, char? Second) Peek2()
    {
        return (_reader.TryPeek(0, out var ch1) ? ch1 : null, _reader.TryPeek(1, out var ch2) ? ch2 : null);
    }

    private void Advance(StringBuilder? builder = null)
    {
        _ = Read(builder);
    }

    private char Read(StringBuilder? builder = null)
    {
        var ch = _reader.Read();

        _ = (builder ?? _chars).Append(ch);

        _position++;

        return ch;
    }

    private void Error(SourceDiagnosticCode code, int position, string message)
    {
        Error(code, position, _position - position, message);
    }

    private void Error(SourceDiagnosticCode code, int position, int length, string message)
    {
        _diagnostics.Add(
            SourceDiagnostic.Create(
                code, SourceDiagnosticSeverity.Error, _text.GetLocation(new(position, length)), message));

        _errors = true;
    }

    private SyntaxTrivia CreateTrivia(int position, SyntaxTriviaKind kind)
    {
        var trivia = new SyntaxTrivia(position, kind, _trivia.ToString());

        _ = _trivia.Clear();

        return trivia;
    }

    private SyntaxToken CreateToken(int position, SyntaxTokenKind kind)
    {
        var text = _chars.ToString();

        _ = _chars.Clear();

        // We handle keywords and nil/Boolean literals here to avoid an extra allocation while lexing identifiers.
        if (kind == SyntaxTokenKind.LowerIdentifier)
        {
            if (SyntaxFacts.GetNormalKeywordKind(text) is { } kw1)
                kind = kw1;
            else if (SyntaxFacts.GetTypeKeywordKind(text) is { } kw2)
                kind = kw2;
            else if (SyntaxFacts.GetReservedKeywordKind(text) is { } kw3)
                kind = kw3;
            else if (SyntaxFacts.GetKeywordLiteralKind(text) is { } lit)
                kind = lit;
        }

        // Intern keywords, nil/Boolean literals, punctuators, and special operators. These all have a fixed textual
        // representation, as opposed to e.g. identifiers and number literals.
        if (SyntaxFacts.IsInternable(kind))
            text = string.Intern(text);

        var token = new SyntaxToken(
            position,
            kind,
            text,
            kind switch
            {
                _ when _errors => null,
                SyntaxTokenKind.NilLiteral => CreateNil(),
                SyntaxTokenKind.BooleanLiteral => CreateBoolean(text),
                SyntaxTokenKind.IntegerLiteral => CreateInteger(position, text),
                SyntaxTokenKind.RealLiteral => CreateReal(position, text),
                SyntaxTokenKind.AtomLiteral => CreateAtom(text),
                SyntaxTokenKind.StringLiteral => CreateString(text),
                _ => null,
            },
            _leading.DrainToImmutable(),
            _trailing.DrainToImmutable());

        _errors = false;

        return token;
    }

    private static object? CreateNil()
    {
        return null;
    }

    private static bool CreateBoolean(string text)
    {
        return text == "true";
    }

    private BigInteger? CreateInteger(int position, string text)
    {
        var radix = 10;

        if (text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            radix = 2;
        else if (text.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
            radix = 8;
        else if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            radix = 16;

        var span = text.AsSpan();

        // Strip base prefix.
        if (radix != 10)
            span = span[2..];

        var result = BigInteger.Zero;

        try
        {
            foreach (var ch in span)
            {
                if (ch == '_')
                    continue;

                result = result * radix + ch switch
                {
                    >= '0' and <= '9' => ch - '0',
                    >= 'a' and <= 'z' => ch - 'a' + 10,
                    >= 'A' and <= 'Z' => ch - 'A' + 10,
                    _ => throw new UnreachableException(),
                };
            }
        }
        catch (Exception ex) when (ex is OutOfMemoryException or OverflowException)
        {
            Error(StandardDiagnosticCodes.InvalidIntegerLiteral, position, text.Length, "Integer literal is too large");

            return null;
        }

        return result;
    }

    private double? CreateReal(int position, string text)
    {
        var value = double.Parse(
            text.Replace("_", null, StringComparison.Ordinal),
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture);

        if (!double.IsInfinity(value))
            return value;

        Error(StandardDiagnosticCodes.InvalidRealLiteral, position, text.Length, "Real literal is out of range");

        return null;
    }

    private static ReadOnlyMemory<char> CreateAtom(string text)
    {
        // Strip colon prefix.
        return text.AsMemory(1..);
    }

    private ReadOnlyMemory<byte> CreateString(string text)
    {
        // TODO: Build up the string in ParseStringLiteral to avoid duplication of escape sequence processing logic.

        using var enumerator = text.GetEnumerator();

        // Skip opening double quote.
        _ = enumerator.MoveNext();

        var hex = (stackalloc char[UnicodeEscapeSequenceLength]);
        var code = (stackalloc char[2]);

        while (enumerator.MoveNext())
        {
            var cur = enumerator.Current;

            // Closing double quote.
            if (cur == '"')
                continue;

            if (cur != '\\')
            {
                _ = _string.Append(cur);

                continue;
            }

            _ = enumerator.MoveNext();

            var replacement = char.MaxValue;

            switch (enumerator.Current)
            {
                case '0':
                    replacement = '\0';
                    break;
                case 'n' or 'N':
                    replacement = '\n';
                    break;
                case 'r' or 'R':
                    replacement = '\r';
                    break;
                case 't' or 'T':
                    replacement = '\t';
                    break;
                case '"':
                    replacement = '"';
                    break;
                case '\\':
                    replacement = '\\';
                    break;
                case 'u' or 'U':
                    break;
            }

            if (replacement == char.MaxValue)
            {
                for (var i = 0; i < hex.Length; i++)
                {
                    _ = enumerator.MoveNext();

                    hex[i] = enumerator.Current;
                }

                var scalar = (Rune)int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

                _ = _string.Append(code[..scalar.EncodeToUtf16(code)]);
            }
            else
                _ = _string.Append(replacement);
        }

        var result = _string.ToString();

        _ = _string.Clear();

        return Encoding.UTF8.GetBytes(result);
    }

    public IReadOnlyList<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();

        // The shebang line can only occur at the very beginning.
        if (_mode == SyntaxMode.Module && Peek2() == ('#', '!'))
            LexShebangLine(_position, _leading);

        while (true)
        {
            LexTrivia(_position, _leading);

            var (position, kind) = Peek1() switch
            {
                '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<' or '=' or '!' =>
                    LexOperatorOrPunctuator(_position),
                '.' => LexPunctuator(_position, SyntaxTokenKind.Dot),
                ',' => LexPunctuator(_position, SyntaxTokenKind.Comma),
                ';' => LexPunctuator(_position, SyntaxTokenKind.Semicolon),
                '@' => LexPunctuator(_position, SyntaxTokenKind.At),
                '#' => LexPunctuator(_position, SyntaxTokenKind.Hash),
                '?' => LexPunctuator(_position, SyntaxTokenKind.Question),
                '(' => LexPunctuator(_position, SyntaxTokenKind.OpenParen),
                ')' => LexPunctuator(_position, SyntaxTokenKind.CloseParen),
                '[' => LexPunctuator(_position, SyntaxTokenKind.OpenBracket),
                ']' => LexPunctuator(_position, SyntaxTokenKind.CloseBracket),
                '{' => LexPunctuator(_position, SyntaxTokenKind.OpenBrace),
                '}' => LexPunctuator(_position, SyntaxTokenKind.CloseBrace),
                >= 'A' and <= 'Z' => LexIdentifier(_position, SyntaxTokenKind.UpperIdentifier),
                >= 'a' and <= 'z' => LexIdentifier(_position, SyntaxTokenKind.LowerIdentifier),
                '_' => LexIdentifier(_position, SyntaxTokenKind.DiscardIdentifier),
                >= '0' and <= '9' => LexNumberLiteral(_position),
                ':' => LexAtomLiteralOrPunctuator(_position),
                '"' => LexStringLiteral(_position),
                null => (_position, SyntaxTokenKind.EndOfInput),
                _ => LexUnrecognized(_position),
            };

            if (kind != SyntaxTokenKind.EndOfInput)
                LexTrivia(_position, _trailing);

            tokens.Add(CreateToken(position, kind));

            if (kind == SyntaxTokenKind.EndOfInput)
                break;
        }

        return tokens;
    }

    private void LexShebangLine(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1() is not (null or '\n' or '\r'))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.ShebangLine));
    }

    private void LexTrivia(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek2() is ({ } ch1, var ch2))
        {
            switch ((ch1, ch2))
            {
                case (' ' or '\t', _):
                    LexWhiteSpace(position, builder);
                    continue;
                case ('\n' or '\r', _):
                    LexNewLine(position, builder);

                    // Trailing trivia for a token stops when the line ends.
                    if (builder == _trailing)
                        break;
                    else
                        continue;
                case ('/', '/'):
                    LexComment(position, builder);
                    continue;
                default:
                    break;
            }

            break;
        }
    }

    private void LexWhiteSpace(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek1() is ' ' or '\t')
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.WhiteSpace));
    }

    private void LexNewLine(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        if ((Read(_trivia), Peek1()) == ('\r', '\n'))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.NewLine));
    }

    private void LexComment(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1() is not (null or '\n' or '\r'))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.Comment));
    }

    private (int Position, SyntaxTokenKind Kind) LexUnrecognized(int position)
    {
        Advance();

        Error(StandardDiagnosticCodes.UnrecognizedCharacter, position, 1, "Unrecognized character");

        return (position, SyntaxTokenKind.Unrecognized);
    }

    private (int Position, SyntaxTokenKind Kind) LexOperatorOrPunctuator(int position)
    {
        var ch1 = Read();
        var ch2 = Peek1();

        // Handle operators that we know cannot be custom operators.
        var kind = (ch1, ch2) switch
        {
            ('!', '=') => SyntaxTokenKind.ExclamationEquals,
            ('<', '=') => SyntaxTokenKind.OpenAngleEquals,
            ('=', '=') => SyntaxTokenKind.EqualsEquals,
            ('=', _) => SyntaxTokenKind.Equals,
            _ => default(SyntaxTokenKind?),
        };

        if (kind is { } k)
        {
            Advance();

            return (position, k);
        }

        if (ch1 == '!')
        {
            Error(StandardDiagnosticCodes.IncompleteExclamationEquals, position, "Incomplete '!=' operator");

            return (position, SyntaxTokenKind.ExclamationEquals);
        }

        var parts = 1;

        // Lex the full operator.
        while (Peek1() is '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<')
        {
            Advance();

            parts++;
        }

        // Handle remaining special operators, and then custom operators.
        return (parts, ch1, ch2) switch
        {
            (1, '<', _) => (position, SyntaxTokenKind.OpenAngle),
            (1, '>', _) => (position, SyntaxTokenKind.CloseAngle),
            (2, '-', '>') => (position, SyntaxTokenKind.MinusCloseAngle),
            (_, '+' or '-' or '~', _) => (position, SyntaxTokenKind.AdditiveOperator),
            (_, '*' or '/' or '%', _) => (position, SyntaxTokenKind.MultiplicativeOperator),
            (_, '&' or '|' or '^', _) => (position, SyntaxTokenKind.BitwiseOperator),
            (_, '>' or '<', _) => (position, SyntaxTokenKind.ShiftOperator),
            _ => throw new UnreachableException(),
        };
    }

    private (int Position, SyntaxTokenKind Kind) LexPunctuator(int position, SyntaxTokenKind kind)
    {
        Advance();

        if (kind != SyntaxTokenKind.Dot || Peek1() != '.')
            return (position, kind);

        Advance();

        return (position, SyntaxTokenKind.DotDot);
    }

    private (int Position, SyntaxTokenKind Kind) LexIdentifier(int position, SyntaxTokenKind kind)
    {
        Advance();

        switch (kind)
        {
            case SyntaxTokenKind.UpperIdentifier:
                while (Peek1() is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'))
                    Advance();

                break;
            case SyntaxTokenKind.LowerIdentifier:
            case SyntaxTokenKind.DiscardIdentifier:
                while (Peek1() is '_' or (>= '0' and <= '9') or (>= 'a' and <= 'z'))
                    Advance();

                break;
        }

        return (position, kind);
    }

    private (int Position, SyntaxTokenKind Kind) LexNumberLiteral(int position)
    {
        var ch1 = Read();
        var ch2 = Peek1();

        var radix = (ch1, ch2) switch
        {
            ('0', 'b') => 2,
            ('0', 'B') => 2,
            ('0', 'o') => 8,
            ('0', 'O') => 8,
            ('0', 'x') => 16,
            ('0', 'X') => 16,
            _ => 10,
        };

        if (radix != 10)
            Advance();

        bool ConsumeDigits(int radix)
        {
            var ok = false;

            while (true)
            {
                switch ((radix, Peek1()))
                {
                    // TODO: https://github.com/dotnet/roslyn/issues/63445
                    case (2 or 8 or 10 or 16, >= '0' and <= '1'):
                    case (8 or 10 or 16, >= '2' and <= '7'):
                    case (10 or 16, >= '8' and <= '9'):
                    case (16, (>= 'a' and <= 'f') or (>= 'A' and <= 'F')):
                        ok = true;

                        Advance();
                        continue;
                    case (_, '_') when ok:
                        ok = false;

                        Advance();
                        continue;
                    default:
                        break;
                }

                break;
            }

            return ok;
        }

        if (!ConsumeDigits(radix) && radix != 10)
        {
            Error(
                StandardDiagnosticCodes.IncompleteIntegerLiteral, position, $"Incomplete base-{radix} integer literal");

            return (position, SyntaxTokenKind.IntegerLiteral);
        }

        // Is it an integer or real literal?
        if (radix != 10 || Peek1() != '.')
            return (position, SyntaxTokenKind.IntegerLiteral);

        Advance();

        if (!ConsumeDigits(10))
        {
            Error(StandardDiagnosticCodes.IncompleteRealLiteral, position, "Incomplete real literal");

            return (position, SyntaxTokenKind.RealLiteral);
        }

        // Do we have an exponent part?
        if (Peek1() is not ('e' or 'E'))
            return (position, SyntaxTokenKind.RealLiteral);

        Advance();

        if (Peek1() is '+' or '-')
            Advance();

        if (!ConsumeDigits(10))
            Error(StandardDiagnosticCodes.IncompleteRealLiteral, position, "Incomplete real literal");

        return (position, SyntaxTokenKind.RealLiteral);
    }

    private (int Position, SyntaxTokenKind Kind) LexAtomLiteralOrPunctuator(int position)
    {
        Advance();

        var ch = Peek1();

        if (ch == ':')
        {
            Advance();

            return (position, SyntaxTokenKind.ColonColon);
        }

        var ident = ch switch
        {
            >= 'A' and <= 'Z' => SyntaxTokenKind.UpperIdentifier,
            >= 'a' and <= 'z' => SyntaxTokenKind.LowerIdentifier,
            '_' => SyntaxTokenKind.DiscardIdentifier,
            _ => default(SyntaxTokenKind?),
        };

        if (ident is { } k)
        {
            _ = LexIdentifier(position, k);

            return (position, SyntaxTokenKind.AtomLiteral);
        }

        return (position, SyntaxTokenKind.Colon);
    }

    private (int Position, SyntaxTokenKind Kind) LexStringLiteral(int position)
    {
        Advance();

        var hex = (stackalloc char[UnicodeEscapeSequenceLength]);

        while (true)
        {
            if (Peek1() is null or '\n' or '\r')
            {
                Error(StandardDiagnosticCodes.IncompleteStringLiteral, position, "Incomplete string literal");

                break;
            }

            var chPos = _position;
            var ch = Read();

            if (ch == '"')
                break;

            if (ch != '\\')
                continue;

            switch (Peek1())
            {
                case '0' or 'n' or 'N' or 'r' or 'R' or 't' or 'T' or '"' or '\\':
                    Advance();
                    break;
                case 'u' or 'U':
                    Advance();

                    var codePos = _position;

                    for (var i = 0; i < UnicodeEscapeSequenceLength; i++)
                    {
                        var digit = Peek1();

                        if (digit is not (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))
                        {
                            Error(
                                StandardDiagnosticCodes.IncompleteUnicodeEscapeSequence,
                                codePos,
                                "Incomplete Unicode escape sequence");

                            break;
                        }

                        hex[i] = Read();
                    }

                    if (!Rune.IsValid(int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)))
                        Error(
                            StandardDiagnosticCodes.InvalidUnicodeEscapeSequence,
                            codePos,
                            UnicodeEscapeSequenceLength,
                            $"Invalid Unicode escape sequence");

                    break;
                default:
                    Error(StandardDiagnosticCodes.IncompleteEscapeSequence, chPos, "Incomplete escape sequence");
                    break;
            }
        }

        return (position, SyntaxTokenKind.StringLiteral);
    }
}
