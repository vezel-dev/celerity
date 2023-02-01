namespace Vezel.Celerity.Syntax;

internal sealed class LanguageLexer
{
    private static readonly int _initialBufferSize = Environment.SystemPageSize;

    private readonly SyntaxInputReader<Rune> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly List<(SourceDiagnosticSeverity, SourceLocation, string)> _currentDiagnostics = new();

    private readonly List<Rune> _runes = new();

    private readonly List<Rune> _trivia = new();

    private readonly List<Rune> _string = new();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _leading = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _trailing = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private SourceLocation _location;

    private byte[] _utf8 = new byte[_initialBufferSize];

    private char[] _utf16 = new char[_initialBufferSize];

    public LanguageLexer(
        string fullPath,
        ReadOnlyMemory<Rune> runes,
        SyntaxMode mode,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _reader = new(runes);
        _mode = mode;
        _diagnostics = diagnostics;
        _location = new(fullPath, 1, 1);
    }

    private Rune? Peek1()
    {
        return _reader.TryPeek(out var rune) ? rune : null;
    }

    private (Rune First, Rune Second)? Peek2()
    {
        return _reader.TryPeek(out var first, out var second) ? (first, second) : null;
    }

    private void Advance(List<Rune>? runes = null)
    {
        _ = Read(runes);
    }

    private Rune Read(List<Rune>? runes = null)
    {
        var rune = _reader.Read();
        var nl = rune.Value == '\n';

        _location = new(_location.FullPath, _location.Line + (nl ? 1 : 0), nl ? 1 : _location.Column + 1);

        (runes ?? _runes).Add(rune);

        return rune;
    }

    private void Error(SourceLocation location, string message)
    {
        _currentDiagnostics.Add((SourceDiagnosticSeverity.Error, location, message));
    }

    private void ErrorExpected(string expected, Rune? found)
    {
        // TODO: Better printing of special runes (white space, control, etc).
        Error(_location, $"Expected {expected}, but found {(found != null ? $"'{found}'" : "end of input")}");
    }

    private ReadOnlyMemory<byte> ToUtf8(List<Rune> runes)
    {
        var length = 0;

        foreach (var rune in runes)
        {
            var span = _utf8.AsSpan(length..);

            int runeLength;

            while (!rune.TryEncodeToUtf8(span, out runeLength))
            {
                Array.Resize(ref _utf8, _utf8.Length * 2);

                span = _utf8.AsSpan(length..);
            }

            length += runeLength;
        }

        return _utf8.AsSpan(..length).ToArray();
    }

    private string ToUtf16(List<Rune> runes)
    {
        var length = 0;

        foreach (var rune in runes)
        {
            var span = _utf16.AsSpan(length..);

            int runeLength;

            while (!rune.TryEncodeToUtf16(span, out runeLength))
            {
                Array.Resize(ref _utf16, _utf16.Length * 2);

                span = _utf16.AsSpan(length..);
            }

            length += runeLength;
        }

        return _utf16.AsSpan(..length).ToString();
    }

    private SyntaxTrivia CreateTrivia(SourceLocation location, SyntaxTriviaKind kind)
    {
        var trivia = new SyntaxTrivia(location, kind, ToUtf16(_trivia));

        _trivia.Clear();

        return trivia;
    }

    private SyntaxToken CreateToken(SourceLocation location, SyntaxTokenKind kind)
    {
        var text = ToUtf16(_runes);
        var token = new SyntaxToken(
            location,
            kind,
            text,
            kind switch
            {
                _ when _currentDiagnostics.Count != 0 => null,
                SyntaxTokenKind.IntegerLiteral => CreateInteger(text),
                SyntaxTokenKind.RealLiteral => CreateReal(text),
                SyntaxTokenKind.AtomLiteral => CreateAtom(text),
                SyntaxTokenKind.StringLiteral => CreateString(),
                _ => null,
            },
            _leading.ToImmutable(),
            _trailing.ToImmutable());

        _runes.Clear();
        _leading.Clear();
        _trailing.Clear();

        _diagnostics.AddRange(
            _currentDiagnostics.Select(tup => SourceDiagnostic.Create(token, tup.Item1, tup.Item2, tup.Item3)));
        _currentDiagnostics.Clear();

        return token;
    }

    private static BigInteger CreateInteger(string text)
    {
        var radix = 10;

        if (text.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            radix = 2;
        else if (text.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
            radix = 8;
        else if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            radix = 16;

        var str = text.AsSpan();

        // Strip base prefix.
        if (radix != 10)
            str = str[2..];

        var result = BigInteger.Zero;

        foreach (var ch in str)
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

        return result;
    }

    private static double CreateReal(string text)
    {
        return double.Parse(
            text.Replace("_", null, StringComparison.Ordinal),
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture);
    }

    private static ReadOnlyMemory<char> CreateAtom(string text)
    {
        return text.AsMemory(1..);
    }

    private ReadOnlyMemory<byte> CreateString()
    {
        using var enumerator = _runes.GetEnumerator();

        // Skip opening double quote.
        _ = enumerator.MoveNext();

        var hex = (stackalloc char[6]);

        while (enumerator.MoveNext())
        {
            var cur = enumerator.Current;

            // Closing double quote.
            if (cur.Value == '"')
                continue;

            if (cur.Value != '\\')
            {
                _string.Add(cur);

                continue;
            }

            _ = enumerator.MoveNext();

            int scalar;

            switch (enumerator.Current.Value)
            {
                case '0':
                    scalar = '\0';
                    break;
                case 'n' or 'N':
                    scalar = '\n';
                    break;
                case 'r' or 'R':
                    scalar = '\r';
                    break;
                case 't' or 'T':
                    scalar = '\t';
                    break;
                case '"':
                    scalar = '"';
                    break;
                case '\\':
                    scalar = '\\';
                    break;
                case 'u' or 'U':
                    for (var i = 0; i < hex.Length; i++)
                    {
                        _ = enumerator.MoveNext();
                        _ = enumerator.Current.EncodeToUtf16(hex[i..]);
                    }

                    scalar = int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new UnreachableException();
            }

            _string.Add((Rune)scalar);
        }

        var result = ToUtf8(_string);

        _string.Clear();

        return result;
    }

    public ReadOnlyMemory<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();

        // The shebang line can only occur at the very beginning.
        if (_mode == SyntaxMode.Document && Peek2() == ((Rune)'#', (Rune)'!'))
            LexShebangLine(_location, _leading);

        while (true)
        {
            LexTrivia(_leading);

            SourceLocation location;
            SyntaxTokenKind kind;

            if (Peek1() is Rune cur)
            {
                (location, kind) = cur.Value switch
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
                    >= 'A' and <= 'Z' => LexIdentifierOrKeyword(_location, SyntaxTokenKind.UpperIdentifier, false),
                    >= 'a' and <= 'z' => LexIdentifierOrKeyword(_location, SyntaxTokenKind.LowerIdentifier, true),
                    '_' => LexIdentifierOrKeyword(_location, SyntaxTokenKind.DiscardIdentifier, false),
                    >= '0' and <= '9' => LexNumberLiteral(_location),
                    ':' => LexAtomLiteralOrPunctuator(_location, SyntaxTokenKind.Colon),
                    '"' => LexStringLiteral(_location),
                    _ => (_location, SyntaxTokenKind.Unrecognized),
                };

                if (kind == SyntaxTokenKind.Unrecognized)
                {
                    Advance();

                    Error(location, $"Unrecognized character '{cur}'");
                }

                LexTrivia(_trailing);
            }
            else
                (location, kind) = (_location, SyntaxTokenKind.EndOfInput);

            tokens.Add(CreateToken(location, kind));

            if (kind == SyntaxTokenKind.EndOfInput)
                break;
        }

        return tokens.ToArray();
    }

    private void LexShebangLine(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder array)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1()?.Value is not ('\r' or '\n'))
            Advance(_trivia);

        array.Add(CreateTrivia(location, SyntaxTriviaKind.ShebangLine));
    }

    private void LexTrivia(ImmutableArray<SyntaxTrivia>.Builder array)
    {
        while (Peek1() is Rune cur)
        {
            switch (cur.Value)
            {
                case ' ':
                case '\t':
                    LexWhiteSpace(_location, array);
                    continue;
                case '\r':
                case '\n':
                    LexNewLine(_location, array);

                    // Trailing trivia for a token stops when the line ends.
                    if (array == _trailing)
                        break;
                    else
                        continue;
                default:
                    if (Peek2() != ((Rune)'/', (Rune)'/'))
                        break;

                    LexComment(_location, array);
                    continue;
            }

            break;
        }
    }

    private void LexWhiteSpace(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder array)
    {
        while (Peek1()?.Value is ' ' or '\t')
            Advance(_trivia);

        array.Add(CreateTrivia(location, SyntaxTriviaKind.WhiteSpace));
    }

    private void LexNewLine(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder array)
    {
        if (Read(_trivia).Value == '\r' && Peek1()?.Value == '\n')
            Advance(_trivia);

        array.Add(CreateTrivia(location, SyntaxTriviaKind.NewLine));
    }

    private void LexComment(SourceLocation location, ImmutableArray<SyntaxTrivia>.Builder array)
    {
        Advance(_trivia);
        Advance(_trivia);

        while (Peek1()?.Value is not ('\r' or '\n'))
            Advance(_trivia);

        array.Add(CreateTrivia(location, SyntaxTriviaKind.Comment));
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexOperatorOrPunctuator(SourceLocation location)
    {
        var r1 = Read();
        var r2 = Peek1();

        // Handle operators that we know cannot be custom operators.
        var kind = (r1.Value, r2?.Value) switch
        {
            ('!', '=') => SyntaxTokenKind.ExclamationEquals,
            ('<', '=') => SyntaxTokenKind.OpenAngleEquals,
            ('=', '=') => SyntaxTokenKind.EqualsEquals,
            ('=', '>') => SyntaxTokenKind.EqualsCloseAngle,
            ('=', _) => SyntaxTokenKind.Equals,
            _ => default(SyntaxTokenKind?),
        };

        if (kind is SyntaxTokenKind k)
            return (location, k);

        if (r1.Value == '!')
        {
            ErrorExpected("'='", r2);

            return (location, SyntaxTokenKind.ExclamationEquals);
        }

        var parts = 1;

        // Lex the full operator.
        while (Peek1()?.Value is '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<')
        {
            Advance();

            parts++;
        }

        // Handle remaining special operators and custom operators.
        return (parts, r1.Value, r2?.Value) switch
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

        if (kind == SyntaxTokenKind.Dot && Peek1()?.Value == '.')
        {
            Advance();

            return (location, SyntaxTokenKind.DotDot);
        }

        return (location, kind);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexIdentifierOrKeyword(
        SourceLocation location, SyntaxTokenKind kind, bool keyword)
    {
        switch (kind)
        {
            case SyntaxTokenKind.UpperIdentifier:
                while (Peek1()?.Value is ('0' and <= '9') or ('a' and <= 'z') or (>= 'A' and <= 'Z'))
                    Advance();

                break;
            case SyntaxTokenKind.LowerIdentifier:
            case SyntaxTokenKind.DiscardIdentifier:
                while (Peek1()?.Value is '_' or ('0' and <= '9') or ('a' and <= 'z'))
                    Advance();

                break;
        }

        if (keyword)
        {
            var text = ToUtf16(_runes);

            if (SyntaxFacts.GetNormalKeywordKind(text) is SyntaxTokenKind kw1)
                kind = kw1;
            else if (SyntaxFacts.GetTypeKeywordKind(text) is SyntaxTokenKind kw2)
                kind = kw2;
            else if (SyntaxFacts.GetReservedKeywordKind(text) is SyntaxTokenKind kw3)
                kind = kw3;
        }

        return (location, kind);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexNumberLiteral(SourceLocation location)
    {
        var radix = (Read().Value, Peek1()?.Value) switch
        {
            ('0', 'b' or 'B') => 2,
            ('0', 'o' or 'O') => 8,
            ('0', 'x' or 'X') => 16,
            _ => 10,
        };

        if (radix != 10)
            Advance();

        bool ConsumeDigits(int radix)
        {
            var good = false;

            while (true)
            {
                switch ((radix, Peek1()?.Value))
                {
                    // TODO: https://github.com/dotnet/roslyn/issues/63445
                    case (2 or 8 or 10 or 16, >= '0' and <= '1'):
                    case (8 or 10 or 16, >= '2' and <= '7'):
                    case (10 or 16, >= '8' and <= '9'):
                    case (16, (>= 'a' and <= 'f') or (>= 'A' and <= 'F')):
                        good = true;

                        Advance();
                        continue;
                    case (_, '_') when good:
                        good = false;

                        Advance();
                        continue;
                    default:
                        break;
                }

                break;
            }

            return good;
        }

        if (!ConsumeDigits(radix) && radix != 10)
        {
            ErrorExpected($"base-{radix} digit", Peek1());

            return (location, SyntaxTokenKind.IntegerLiteral);
        }

        // Is it an integer or real literal?
        if (radix != 10 || Peek1()?.Value != '.')
            return (location, SyntaxTokenKind.IntegerLiteral);

        Advance();

        if (!ConsumeDigits(10))
        {
            ErrorExpected("digit", Peek1());

            return (location, SyntaxTokenKind.RealLiteral);
        }

        // Do we have an exponent part?
        if (Peek1()?.Value is not ('e' or 'E'))
            return (_location, SyntaxTokenKind.RealLiteral);

        Advance();

        if (Peek1()?.Value is '+' or '-')
            Advance();

        if (!ConsumeDigits(10))
            ErrorExpected("digit", Peek1());

        return (_location, SyntaxTokenKind.RealLiteral);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexAtomLiteralOrPunctuator(
        SourceLocation location, SyntaxTokenKind kind)
    {
        Advance();

        if (kind == SyntaxTokenKind.Colon && Peek1()?.Value == ':')
        {
            Advance();

            return (location, SyntaxTokenKind.ColonColon);
        }

        var ident = Peek1()?.Value switch
        {
            >= 'A' and <= 'Z' => SyntaxTokenKind.UpperIdentifier,
            >= 'a' and <= 'z' => SyntaxTokenKind.LowerIdentifier,
            '_' => SyntaxTokenKind.DiscardIdentifier,
            _ => default(SyntaxTokenKind?),
        };

        if (ident is SyntaxTokenKind k)
        {
            kind = SyntaxTokenKind.AtomLiteral;

            _ = LexIdentifierOrKeyword(location, k, false);
        }

        return (location, kind);
    }

    private (SourceLocation Location, SyntaxTokenKind Kind) LexStringLiteral(SourceLocation location)
    {
        Advance();

        while (true)
        {
            var cur = Peek1();

            if (cur?.Value is null or '\r' or '\t')
            {
                ErrorExpected("closing '\"'", cur);

                break;
            }

            var r = Read();

            if (r.Value == '"')
                break;

            if (r.Value != '\\')
                continue;

            var code = Peek1();

            switch (code?.Value)
            {
                case '0' or 'n' or 'N' or 'r' or 'R' or 't' or 'T' or '"' or '\\':
                    Advance();
                    break;
                case 'u' or 'U':
                    Advance();

                    for (var i = 0; i < 6; i++)
                    {
                        var hex = Peek1();

                        if (hex?.Value is not (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))
                        {
                            ErrorExpected("Unicode escape sequence digit", hex);

                            break;
                        }

                        Advance();
                    }

                    break;
                default:
                    ErrorExpected("escape sequence code", code);
                    break;
            }
        }

        return (location, SyntaxTokenKind.StringLiteral);
    }
}
