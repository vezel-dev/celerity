using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

internal sealed class LanguageLexer
{
    private static readonly Encoding _utf8 = Encoding.UTF8;

    private readonly ListReader<char> _reader;

    private readonly SyntaxMode _mode;

    private readonly List<Func<SyntaxTree, Diagnostic>> _diagnostics;

    private readonly StringBuilder _chars = new();

    private readonly StringBuilder _trivia = new();

    private readonly List<char> _string = new();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _leading = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _trailing = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private bool _errors;

    private bool _whiteSpaceDiagnostic;

    private bool _newLineDiagnostic;

    public LanguageLexer(SourceText text, SyntaxMode mode, List<Func<SyntaxTree, Diagnostic>> diagnostics)
    {
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

        return ch;
    }

    private void TokenError(int position, DiagnosticCode code, string message)
    {
        TokenError(position, _reader.Position - position, code, message);
    }

    private void TokenError(int position, int length, DiagnosticCode code, string message)
    {
        _diagnostics.Add(tree =>
            new(
                tree,
                new(position, length),
                code,
                DiagnosticSeverity.Error,
                message,
                ImmutableArray<DiagnosticNote>.Empty));

        _errors = true;
    }

    private void TriviaError(ref bool flag, int position, DiagnosticCode code, string message)
    {
        if (flag)
            return;

        _diagnostics.Add(tree =>
            new(
                tree,
                new(position, 1),
                code,
                DiagnosticSeverity.Error,
                message,
                ImmutableArray<DiagnosticNote>.Empty));

        flag = true;
    }

    private SyntaxTrivia CreateTrivia(int position, SyntaxTriviaKind kind)
    {
        var text = _trivia.ToString();

        _ = _trivia.Clear();

        return new SyntaxTrivia(
            position,
            kind,
            kind == SyntaxTriviaKind.NewLine ? string.Intern(text) : text);
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
                SyntaxTokenKind.StringLiteral => CreateString(),
                _ => null,
            },
            new(_leading.DrainToImmutable()),
            new(_trailing.DrainToImmutable()));

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
            TokenError(
                position, text.Length, StandardDiagnosticCodes.InvalidIntegerLiteral, "Integer literal is too large");

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

        TokenError(position, text.Length, StandardDiagnosticCodes.InvalidRealLiteral, "Real literal is out of range");

        return null;
    }

    private static ReadOnlyMemory<char> CreateAtom(string text)
    {
        // Strip colon prefix.
        return text.AsMemory(1..);
    }

    private ReadOnlyMemory<byte> CreateString()
    {
        var chars = CollectionsMarshal.AsSpan(_string);
        var bytes = new byte[_utf8.GetByteCount(chars)];

        _ = _utf8.GetBytes(chars, bytes);

        return bytes;
    }

    public IReadOnlyList<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();

        // The shebang line can only occur at the very beginning.
        if (_mode == SyntaxMode.Module && Peek2() == ('#', '!'))
            LexShebangLine(_reader.Position, _leading);

        while (true)
        {
            LexTrivia(_leading);

            var position = _reader.Position;
            var kind = Peek1() switch
            {
                '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<' or '=' or '!' =>
                    LexOperatorOrPunctuator(position),
                '.' => LexPunctuator(SyntaxTokenKind.Dot),
                ',' => LexPunctuator(SyntaxTokenKind.Comma),
                ';' => LexPunctuator(SyntaxTokenKind.Semicolon),
                '@' => LexPunctuator(SyntaxTokenKind.At),
                '#' => LexPunctuator(SyntaxTokenKind.Hash),
                '?' => LexPunctuator(SyntaxTokenKind.Question),
                '(' => LexPunctuator(SyntaxTokenKind.OpenParen),
                ')' => LexPunctuator(SyntaxTokenKind.CloseParen),
                '[' => LexPunctuator(SyntaxTokenKind.OpenBracket),
                ']' => LexPunctuator(SyntaxTokenKind.CloseBracket),
                '{' => LexPunctuator(SyntaxTokenKind.OpenBrace),
                '}' => LexPunctuator(SyntaxTokenKind.CloseBrace),
                >= 'A' and <= 'Z' => LexIdentifier(SyntaxTokenKind.UpperIdentifier),
                >= 'a' and <= 'z' => LexIdentifier(SyntaxTokenKind.LowerIdentifier),
                '_' => LexIdentifier(SyntaxTokenKind.DiscardIdentifier),
                >= '0' and <= '9' => LexNumberLiteral(position),
                ':' => LexAtomLiteralOrPunctuator(),
                '"' => LexStringLiteral(position),
                null => SyntaxTokenKind.EndOfInput,
                _ => LexUnrecognized(position),
            };

            if (kind != SyntaxTokenKind.EndOfInput)
                LexTrivia(_trailing);

            tokens.Add(CreateToken(position, kind));

            if (kind == SyntaxTokenKind.EndOfInput)
                break;
        }

        return tokens;
    }

    private void LexShebangLine(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        for (var i = 0; i < 2; i++)
            Advance(_trivia);

        while (Peek1() is { } ch && !TextFacts.IsNewLine(ch))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.ShebangLine));
    }

    private void LexTrivia(ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek2() is ({ } ch1, var ch2))
        {
            var position = _reader.Position;

            switch ((ch1, ch2))
            {
                case ('/', '/'):
                    LexComment(position, builder);
                    continue;
                default:
                    if (TextFacts.IsWhiteSpace(ch1))
                    {
                        LexWhiteSpace(position, builder);
                        continue;
                    }

                    if (TextFacts.IsNewLine(ch1))
                    {
                        LexNewLine(position, builder);

                        // Trailing trivia for a token stops when the line ends.
                        if (builder != _trailing)
                            continue;
                    }

                    break;
            }

            break;
        }
    }

    private void LexWhiteSpace(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        while (Peek1() is { } ch && TextFacts.IsWhiteSpace(ch))
        {
            if (ch != ' ')
                TriviaError(
                    ref _whiteSpaceDiagnostic,
                    _reader.Position,
                    StandardDiagnosticCodes.UnsupportedWhiteSpaceCharacter,
                    "Input is using unsupported white space characters (only ASCII space is supported)");

            Advance(_trivia);
        }

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.WhiteSpace));
    }

    private void LexNewLine(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        var ch = Read(_trivia);

        if ((ch, Peek1()) == ('\r', '\n'))
            Advance(_trivia);
        else if (ch is not ('\n' or '\r'))
            TriviaError(
                ref _newLineDiagnostic,
                position,
                StandardDiagnosticCodes.UnsupportedNewLineCharacter,
                "Input is using unsupported new-line characters (only LF and CRLF are supported)");

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.NewLine));
    }

    private void LexComment(int position, ImmutableArray<SyntaxTrivia>.Builder builder)
    {
        for (var i = 0; i < 2; i++)
            Advance(_trivia);

        while (Peek1() is { } ch && !TextFacts.IsNewLine(ch))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.Comment));
    }

    private SyntaxTokenKind LexUnrecognized(int position)
    {
        Advance();

        TokenError(position, 1, StandardDiagnosticCodes.UnrecognizedCharacter, "Unrecognized character");

        return SyntaxTokenKind.Unrecognized;
    }

    private SyntaxTokenKind LexOperatorOrPunctuator(int position)
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

            return k;
        }

        if (ch1 == '!')
        {
            TokenError(position, StandardDiagnosticCodes.IncompleteExclamationEquals, "Incomplete '!=' operator");

            return SyntaxTokenKind.ExclamationEquals;
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
            (1, '<', _) => SyntaxTokenKind.OpenAngle,
            (1, '>', _) => SyntaxTokenKind.CloseAngle,
            (2, '-', '>') => SyntaxTokenKind.MinusCloseAngle,
            (_, '+' or '-' or '~', _) => SyntaxTokenKind.AdditiveOperator,
            (_, '*' or '/' or '%', _) => SyntaxTokenKind.MultiplicativeOperator,
            (_, '&' or '|' or '^', _) => SyntaxTokenKind.BitwiseOperator,
            (_, '>' or '<', _) => SyntaxTokenKind.ShiftOperator,
            _ => throw new UnreachableException(),
        };
    }

    private SyntaxTokenKind LexPunctuator(SyntaxTokenKind kind)
    {
        Advance();

        if (kind != SyntaxTokenKind.Dot || Peek1() != '.')
            return kind;

        Advance();

        return SyntaxTokenKind.DotDot;
    }

    private SyntaxTokenKind LexIdentifier(SyntaxTokenKind kind)
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

        return kind;
    }

    private SyntaxTokenKind LexNumberLiteral(int position)
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
            TokenError(
                position, StandardDiagnosticCodes.IncompleteIntegerLiteral, $"Incomplete base-{radix} integer literal");

            return SyntaxTokenKind.IntegerLiteral;
        }

        // Is it an integer or real literal?
        if (radix != 10 || Peek1() != '.')
            return SyntaxTokenKind.IntegerLiteral;

        Advance();

        if (!ConsumeDigits(10))
        {
            TokenError(position, StandardDiagnosticCodes.IncompleteRealLiteral, "Incomplete real literal");

            return SyntaxTokenKind.RealLiteral;
        }

        // Do we have an exponent part?
        if (Peek1() is not ('e' or 'E'))
            return SyntaxTokenKind.RealLiteral;

        Advance();

        if (Peek1() is '+' or '-')
            Advance();

        if (!ConsumeDigits(10))
            TokenError(position, StandardDiagnosticCodes.IncompleteRealLiteral, "Incomplete real literal");

        return SyntaxTokenKind.RealLiteral;
    }

    private SyntaxTokenKind LexAtomLiteralOrPunctuator()
    {
        Advance();

        var ch = Peek1();

        if (ch == ':')
        {
            Advance();

            return SyntaxTokenKind.ColonColon;
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
            _ = LexIdentifier(k);

            return SyntaxTokenKind.AtomLiteral;
        }

        return SyntaxTokenKind.Colon;
    }

    private SyntaxTokenKind LexStringLiteral(int position)
    {
        Advance();

        var hex = (stackalloc char[6]);
        var code = (stackalloc char[2]);

        while (true)
        {
            if (Peek1() is not { } ch || TextFacts.IsNewLine(ch))
            {
                TokenError(position, StandardDiagnosticCodes.IncompleteStringLiteral, "Incomplete string literal");

                break;
            }

            var chPos = _reader.Position;

            Advance();

            if (ch == '"')
                break;

            if (ch != '\\')
            {
                _string.Add(ch);

                continue;
            }

            var replacement = default(char?);

            switch (Peek1())
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
                case ('"' or '\\') and var rep:
                    replacement = rep;
                    break;
                case 'u' or 'U':
                    Advance();

                    var codePos = _reader.Position;

                    for (var i = 0; i < hex.Length; i++)
                    {
                        if (Peek1() is not ((>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F')))
                        {
                            TokenError(
                                codePos,
                                StandardDiagnosticCodes.IncompleteUnicodeEscapeSequence,
                                "Incomplete Unicode escape sequence");

                            break;
                        }

                        hex[i] = Read();
                    }

                    // Did we read the full scalar number?
                    if (_reader.Position != codePos + hex.Length)
                        break;

                    var scalar = int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

                    if (!Rune.TryCreate(scalar, out var rune))
                    {
                        TokenError(
                            codePos,
                            hex.Length,
                            StandardDiagnosticCodes.InvalidUnicodeEscapeSequence,
                            $"Invalid Unicode escape sequence");

                        break;
                    }

                    _string.AddRange(code[..rune.EncodeToUtf16(code)]);

                    break;
                default:
                    TokenError(chPos, StandardDiagnosticCodes.IncompleteEscapeSequence, "Incomplete escape sequence");
                    break;
            }

            if (replacement is not { } repCh)
                continue;

            Advance();

            _string.Add(repCh);
        }

        // If an error occurred, we will not try to create the string value in CreateToken, so just drop it.
        if (_errors)
            _string.Clear();

        return SyntaxTokenKind.StringLiteral;
    }
}
