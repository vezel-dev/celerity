using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax;

internal sealed class LanguageLexer
{
    private readonly ListReader<char> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly StringBuilder _chars = new();

    private readonly StringBuilder _trivia = new();

    private readonly StringBuilder _string = new();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _leading = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _trailing = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private SourceLocation _location;

    private bool _errors;

    public LanguageLexer(SourceText text, SyntaxMode mode, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _reader = new(text);
        _mode = mode;
        _diagnostics = diagnostics;
        _location = new(text.Path, 1, 1);
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

        // Line is incremented in the LexNewLine method as it is easier to handle different end-of-line sequences there.
        _location = new(_location.Path, _location.Line, _location.Character + 1);

        return ch;
    }

    private void Diagnostic(
        SourceDiagnosticCode code, SourceDiagnosticSeverity severity, SourceLocation location, string message)
    {
        _diagnostics.Add(SourceDiagnostic.Create(code, severity, location, message));

        if (severity == SourceDiagnosticSeverity.Error)
            _errors = true;
    }

    private void Warning(SourceDiagnosticCode code, SourceLocation location, string message)
    {
        Diagnostic(code, SourceDiagnosticSeverity.Warning, location, message);
    }

    private void Error(SourceDiagnosticCode code, SourceLocation location, string message)
    {
        Diagnostic(code, SourceDiagnosticSeverity.Error, location, message);
    }

    private void ErrorExpected(SourceDiagnosticCode code, string expected)
    {
        Error(code, _location, $"Expected {expected}");
    }

    private SyntaxTrivia CreateTrivia(SourceLocation location, SyntaxTriviaKind kind)
    {
        var trivia = new SyntaxTrivia(location, kind, _trivia.ToString());

        _ = _trivia.Clear();

        return trivia;
    }

    private SyntaxToken CreateToken(SourceLocation location, SyntaxTokenKind kind)
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
            location,
            kind,
            text,
            kind switch
            {
                _ when _errors => null,
                SyntaxTokenKind.NilLiteral => CreateNil(),
                SyntaxTokenKind.BooleanLiteral => CreateBoolean(text),
                SyntaxTokenKind.IntegerLiteral => CreateInteger(location, text),
                SyntaxTokenKind.RealLiteral => CreateReal(location, text),
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

    private BigInteger? CreateInteger(SourceLocation location, string text)
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
            Error(SyntaxDiagnosticCodes.InvalidIntegerLiteral, location, "Integer literal is too large");

            return null;
        }

        return result;
    }

    private double? CreateReal(SourceLocation location, string text)
    {
        var value = double.Parse(
            text.Replace("_", null, StringComparison.Ordinal),
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture);

        if (!double.IsInfinity(value))
            return value;

        Error(SyntaxDiagnosticCodes.InvalidRealLiteral, location, "Real literal is out of range");

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

        var hex = (stackalloc char[6]);
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
            LexShebangLine(_location, _leading);

        while (true)
        {
            LexTrivia(_location, _leading);

            var (location, kind) = Peek1() switch
            {
                '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<' or '=' or '!' =>
                    LexOperatorOrPunctuator(_location),
                '.' => LexPunctuator(_location, SyntaxTokenKind.Dot),
                ',' => LexPunctuator(_location, SyntaxTokenKind.Comma),
                ';' => LexPunctuator(_location, SyntaxTokenKind.Semicolon),
                '@' => LexPunctuator(_location, SyntaxTokenKind.At),
                '#' => LexPunctuator(_location, SyntaxTokenKind.Hash),
                '?' => LexPunctuator(_location, SyntaxTokenKind.Question),
                '(' => LexPunctuator(_location, SyntaxTokenKind.OpenParen),
                ')' => LexPunctuator(_location, SyntaxTokenKind.CloseParen),
                '[' => LexPunctuator(_location, SyntaxTokenKind.OpenBracket),
                ']' => LexPunctuator(_location, SyntaxTokenKind.CloseBracket),
                '{' => LexPunctuator(_location, SyntaxTokenKind.OpenBrace),
                '}' => LexPunctuator(_location, SyntaxTokenKind.CloseBrace),
                >= 'A' and <= 'Z' => LexIdentifier(_location, SyntaxTokenKind.UpperIdentifier),
                >= 'a' and <= 'z' => LexIdentifier(_location, SyntaxTokenKind.LowerIdentifier),
                '_' => LexIdentifier(_location, SyntaxTokenKind.DiscardIdentifier),
                >= '0' and <= '9' => LexNumberLiteral(_location),
                ':' => LexAtomLiteralOrPunctuator(_location),
                '"' => LexStringLiteral(_location),
                null => (_location, SyntaxTokenKind.EndOfInput),
                _ => LexUnrecognized(_location),
            };

            if (kind != SyntaxTokenKind.EndOfInput)
                LexTrivia(_location, _trailing);

            tokens.Add(CreateToken(location, kind));

            if (kind == SyntaxTokenKind.EndOfInput)
                break;
        }

        return tokens;
    }

    private void LexShebangLine(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1() is not (null or '\n' or '\r'))
            Advance(_trivia);

        builder.Add(CreateTrivia(location, SyntaxTriviaKind.ShebangLine));
    }

    private void LexTrivia(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek2() is ({ } ch1, var ch2))
        {
            switch ((ch1, ch2))
            {
                case (' ' or '\t', _):
                    LexWhiteSpace(location, builder);
                    continue;
                case ('\n' or '\r', _):
                    LexNewLine(location, builder);

                    // Trailing trivia for a token stops when the line ends.
                    if (builder == _trailing)
                        break;
                    else
                        continue;
                case ('/', '/'):
                    LexComment(location, builder);
                    continue;
                default:
                    break;
            }

            break;
        }
    }

    private void LexWhiteSpace(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek1() is ' ' or '\t')
            Advance(_trivia);

        builder.Add(CreateTrivia(location, SyntaxTriviaKind.WhiteSpace));
    }

    private void LexNewLine(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        if ((Read(_trivia), Peek1()) == ('\r', '\n'))
            Advance(_trivia);

        // Character is incremented in the Read method.
        _location = new(_location.Path, _location.Line + 1, 1);

        builder.Add(CreateTrivia(location, SyntaxTriviaKind.NewLine));
    }

    private void LexComment(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1() is not (null or '\n' or '\r'))
            Advance(_trivia);

        builder.Add(CreateTrivia(location, SyntaxTriviaKind.Comment));
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexUnrecognized(SourceLocation location)
    {
        Advance();

        Error(SyntaxDiagnosticCodes.UnrecognizedCharacter, location, $"Unrecognized character");

        return (location, SyntaxTokenKind.Unrecognized);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexOperatorOrPunctuator(SourceLocation location)
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

            return (location, k);
        }

        if (ch1 == '!')
        {
            ErrorExpected(SyntaxDiagnosticCodes.IncompleteExclamationEquals, "'='");

            return (location, SyntaxTokenKind.ExclamationEquals);
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
            (1, '<', _) => (location, SyntaxTokenKind.OpenAngle),
            (1, '>', _) => (location, SyntaxTokenKind.CloseAngle),
            (2, '-', '>') => (location, SyntaxTokenKind.MinusCloseAngle),
            (_, '+' or '-' or '~', _) => (location, SyntaxTokenKind.AdditiveOperator),
            (_, '*' or '/' or '%', _) => (location, SyntaxTokenKind.MultiplicativeOperator),
            (_, '&' or '|' or '^', _) => (location, SyntaxTokenKind.BitwiseOperator),
            (_, '>' or '<', _) => (location, SyntaxTokenKind.ShiftOperator),
            _ => throw new UnreachableException(),
        };
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexPunctuator(SourceLocation location, SyntaxTokenKind kind)
    {
        Advance();

        if (kind != SyntaxTokenKind.Dot || Peek1() != '.')
            return (location, kind);

        Advance();

        return (location, SyntaxTokenKind.DotDot);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexIdentifier(SourceLocation location, SyntaxTokenKind kind)
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

        return (location, kind);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexNumberLiteral(SourceLocation location)
    {
        var ch1 = Read();
        var ch2 = Peek1();

        var (radix, warn) = (ch1, ch2) switch
        {
            ('0', 'b') => (2, false),
            ('0', 'B') => (2, true),
            ('0', 'o') => (8, false),
            ('0', 'O') => (8, true),
            ('0', 'x') => (16, false),
            ('0', 'X') => (16, true),
            _ => (10, false),
        };

        if (radix != 10)
        {
            var loc = _location;
            var ind = Read();

            if (warn)
                Warning(
                    SyntaxDiagnosticCodes.LowercaseBaseIndicator,
                    loc,
                    $"Consider using lowercase base indicator '{char.ToLowerInvariant(ind)}' for clarity");
        }

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
            ErrorExpected(SyntaxDiagnosticCodes.IncompleteIntegerLiteral, $"base-{radix} digit");

            return (location, SyntaxTokenKind.IntegerLiteral);
        }

        // Is it an integer or real literal?
        if (radix != 10 || Peek1() != '.')
            return (location, SyntaxTokenKind.IntegerLiteral);

        Advance();

        if (!ConsumeDigits(10))
        {
            ErrorExpected(SyntaxDiagnosticCodes.IncompleteRealLiteral, "digit");

            return (location, SyntaxTokenKind.RealLiteral);
        }

        // Do we have an exponent part?
        if (Peek1() is not ('e' or 'E'))
            return (location, SyntaxTokenKind.RealLiteral);

        Advance();

        if (Peek1() is '+' or '-')
            Advance();

        if (!ConsumeDigits(10))
            ErrorExpected(SyntaxDiagnosticCodes.IncompleteRealLiteral, "digit");

        return (location, SyntaxTokenKind.RealLiteral);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexAtomLiteralOrPunctuator(SourceLocation location)
    {
        Advance();

        var ch = Peek1();

        if (ch == ':')
        {
            Advance();

            return (location, SyntaxTokenKind.ColonColon);
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
            _ = LexIdentifier(location, k);

            return (location, SyntaxTokenKind.AtomLiteral);
        }

        return (location, SyntaxTokenKind.Colon);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexStringLiteral(SourceLocation location)
    {
        Advance();

        var hex = (stackalloc char[6]);

        while (true)
        {
            if (Peek1() is null or '\n' or '\r')
            {
                ErrorExpected(SyntaxDiagnosticCodes.UnclosedStringLiteral, "closing '\"'");

                break;
            }

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

                    var loc = _location;

                    for (var i = 0; i < 6; i++)
                    {
                        var digit = Peek1();

                        if (digit is not (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))
                        {
                            ErrorExpected(
                                SyntaxDiagnosticCodes.IncompleteUnicodeEscapeSequence, "Unicode escape sequence digit");

                            break;
                        }

                        hex[i] = Read();
                    }

                    if (!Rune.IsValid(int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)))
                        Error(
                            SyntaxDiagnosticCodes.InvalidUnicodeEscapeSequence,
                            loc,
                            $"Invalid Unicode escape sequence");

                    break;
                default:
                    ErrorExpected(SyntaxDiagnosticCodes.IncompleteEscapeSequence, "escape sequence code");
                    break;
            }
        }

        return (location, SyntaxTokenKind.StringLiteral);
    }
}
