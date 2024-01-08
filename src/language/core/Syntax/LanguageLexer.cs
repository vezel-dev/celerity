using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

internal sealed class LanguageLexer
{
    private static readonly Encoding _utf8 = Encoding.UTF8;

    private static readonly object _boxedNil = Nil.Value;

    private static readonly object _boxedTrue = true;

    private static readonly object _boxedFalse = false;

    private readonly ListReader<char> _reader;

    private readonly SyntaxMode _mode;

    private readonly List<Func<SyntaxTree, Diagnostic>> _diagnostics;

    private readonly StringBuilder _token = new();

    private readonly List<char> _string = [];

    private readonly StringBuilder _closingIndentation = new();

    private readonly StringBuilder _currentIndentation = new();

    private readonly StringBuilder _trivia = new();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _leading = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private readonly ImmutableArray<SyntaxTrivia>.Builder _trailing = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    private bool _errors;

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

    private (char? First, char? Second, char? Third) Peek3()
    {
        return (_reader.TryPeek(0, out var ch1) ? ch1 : null,
                _reader.TryPeek(1, out var ch2) ? ch2 : null,
                _reader.TryPeek(2, out var ch3) ? ch3 : null);
    }

    private void Advance(StringBuilder? builder)
    {
        _ = Read(builder);
    }

    private char Read(StringBuilder? builder)
    {
        var ch = _reader.Read();

        _ = builder?.Append(ch);

        return ch;
    }

    private void Error(int position, DiagnosticCode code, string message)
    {
        Error(position, _reader.Position - position, code, message);
    }

    private void Error(int position, int length, DiagnosticCode code, string message)
    {
        _diagnostics.Add(tree => new(tree, new(position, length), DiagnosticSeverity.Error, code, message, []));

        _errors = true;
    }

    private SyntaxTrivia CreateTrivia(int position, SyntaxTriviaKind kind)
    {
        var text = _trivia.ToString();

        _ = _trivia.Clear();

        return new SyntaxTrivia(
            position,
            kind,
            SyntaxFacts.IsInternable(kind) ? string.Intern(text) : text);
    }

    private SyntaxToken CreateToken(int position, SyntaxTokenKind kind)
    {
        var text = _token.ToString();

        _ = _token.Clear();

        // We handle keywords and nil/Boolean literals here to avoid an extra allocation while lexing identifiers.
        if (kind == SyntaxTokenKind.LowerIdentifier)
        {
            if (SyntaxFacts.GetRegularKeywordKind(text) is { } kw1)
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

        _string.Clear();

        return token;
    }

    private static object CreateNil()
    {
        return _boxedNil;
    }

    private static object CreateBoolean(string text)
    {
        return text == "true" ? _boxedTrue : _boxedFalse;
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
            Error(
                position, text.Length, StandardDiagnosticCodes.InvalidIntegerLiteral, "Integer literal is too large");

            return null;
        }

        return result;
    }

    private double? CreateReal(int position, string text)
    {
        var value = double.Parse(
            text.Replace("_", newValue: null, StringComparison.Ordinal),
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture);

        if (!double.IsInfinity(value))
            return value;

        Error(position, text.Length, StandardDiagnosticCodes.InvalidRealLiteral, "Real literal is out of range");

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

        _string.Clear();

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
                null => SyntaxTokenKind.EndOfInput,
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
                >= 'A' and <= 'Z' =>
                    LexIdentifierOrRawStringLiteral(position, SyntaxTokenKind.UpperIdentifier, allowRawString: false),
                >= 'a' and <= 'z' =>
                    LexIdentifierOrRawStringLiteral(position, SyntaxTokenKind.LowerIdentifier, allowRawString: true),
                '_' =>
                    LexIdentifierOrRawStringLiteral(position, SyntaxTokenKind.DiscardIdentifier, allowRawString: false),
                >= '0' and <= '9' => LexNumberLiteral(position),
                ':' => LexAtomLiteralOrPunctuator(position),
                '"' => LexStringLiteral(position),
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
        for (var i = 0; i < 2; i++)
            Advance(_trivia);

        while (Peek1() is { } ch && !TextFacts.IsNewLine(ch))
            Advance(_trivia);

        builder.Add(CreateTrivia(position, SyntaxTriviaKind.Comment));
    }

    private SyntaxTokenKind LexUnrecognized(int position)
    {
        Advance(_token);

        Error(position, 1, StandardDiagnosticCodes.UnrecognizedCharacter, "Unrecognized character");

        return SyntaxTokenKind.Unrecognized;
    }

    private SyntaxTokenKind LexOperatorOrPunctuator(int position)
    {
        var ch1 = Read(_token);
        var ch2 = Peek1();

        // Handle operators that we know cannot be custom operators.
        var kind = (ch1, ch2) switch
        {
            ('!', '=') => SyntaxTokenKind.ExclamationEquals,
            ('>', '=') => SyntaxTokenKind.CloseAngleEquals,
            ('<', '=') => SyntaxTokenKind.OpenAngleEquals,
            ('=', '=') => SyntaxTokenKind.EqualsEquals,
            ('=', _) => SyntaxTokenKind.Equals,
            _ => default(SyntaxTokenKind?),
        };

        if (kind is { } k)
        {
            if (k != SyntaxTokenKind.Equals)
                Advance(_token);

            return k;
        }

        if (ch1 == '!')
        {
            Error(position, StandardDiagnosticCodes.IncompleteExclamationEquals, "Incomplete '!=' operator");

            return SyntaxTokenKind.ExclamationEquals;
        }

        var parts = 1;

        // Lex the full operator.
        for (; Peek1() is '+' or '-' or '~' or '*' or '/' or '%' or '&' or '|' or '^' or '>' or '<'; parts++)
            Advance(_token);

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
        Advance(_token);

        if (kind != SyntaxTokenKind.Dot || Peek1() != '.')
            return kind;

        Advance(_token);

        return SyntaxTokenKind.DotDot;
    }

    private SyntaxTokenKind LexIdentifierOrRawStringLiteral(int position, SyntaxTokenKind kind, bool allowRawString)
    {
        Advance(_token);

        switch (kind)
        {
            case SyntaxTokenKind.UpperIdentifier:
                while (Peek1() is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'))
                    Advance(_token);

                break;
            case SyntaxTokenKind.LowerIdentifier:
            case SyntaxTokenKind.DiscardIdentifier:
                while (Peek1() is '_' or (>= '0' and <= '9') or (>= 'a' and <= 'z'))
                    Advance(_token);

                break;
        }

        return allowRawString && kind == SyntaxTokenKind.LowerIdentifier && Peek3() == ('"', '"', '"')
            ? LexRawStringLiteral(position)
            : kind;
    }

    private SyntaxTokenKind LexNumberLiteral(int position)
    {
        var ch1 = Read(_token);
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
            Advance(_token);

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

                        Advance(_token);
                        continue;
                    case (_, '_') when ok:
                        ok = false;

                        Advance(_token);
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
                position, StandardDiagnosticCodes.IncompleteIntegerLiteral, $"Incomplete base-{radix} integer literal");

            return SyntaxTokenKind.IntegerLiteral;
        }

        // Is it an integer or real literal?
        if (radix != 10 || Peek1() != '.')
            return SyntaxTokenKind.IntegerLiteral;

        Advance(_token);

        if (!ConsumeDigits(10))
        {
            Error(position, StandardDiagnosticCodes.IncompleteRealLiteral, "Incomplete real literal");

            return SyntaxTokenKind.RealLiteral;
        }

        // Do we have an exponent part?
        if (Peek1() is not ('e' or 'E'))
            return SyntaxTokenKind.RealLiteral;

        Advance(_token);

        if (Peek1() is '+' or '-')
            Advance(_token);

        if (!ConsumeDigits(10))
            Error(position, StandardDiagnosticCodes.IncompleteRealLiteral, "Incomplete real literal");

        return SyntaxTokenKind.RealLiteral;
    }

    private SyntaxTokenKind LexAtomLiteralOrPunctuator(int position)
    {
        Advance(_token);

        var ch = Peek1();

        if (ch == ':')
        {
            Advance(_token);

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
            _ = LexIdentifierOrRawStringLiteral(position, k, allowRawString: false);

            return SyntaxTokenKind.AtomLiteral;
        }

        return SyntaxTokenKind.Colon;
    }

    private SyntaxTokenKind LexStringLiteral(int position)
    {
        if (Peek3() == ('"', '"', '"'))
            return LexRawStringLiteral(position);

        Advance(_token);

        var hex = (stackalloc char[6]);
        var code = (stackalloc char[2]);

        while (true)
        {
            if (Peek1() is not { } ch || TextFacts.IsNewLine(ch))
            {
                Error(position, StandardDiagnosticCodes.IncompleteStringLiteral, "Incomplete string literal");

                break;
            }

            var chPos = _reader.Position;

            Advance(_token);

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
                case 'a' or 'A':
                    replacement = '\a';
                    break;
                case 'b' or 'B':
                    replacement = '\b';
                    break;
                case 'e' or 'E':
                    replacement = '\u001b';
                    break;
                case 'f' or 'F':
                    replacement = '\f';
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
                case 'v' or 'V':
                    replacement = '\v';
                    break;
                case ('"' or '\\') and var rep:
                    replacement = rep;
                    break;
                case 'u' or 'U':
                    Advance(_token);

                    var codePos = _reader.Position;

                    for (var i = 0; i < hex.Length; i++)
                    {
                        if (Peek1() is not ((>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F')))
                        {
                            Error(
                                chPos,
                                StandardDiagnosticCodes.IncompleteUnicodeEscapeSequence,
                                "Incomplete Unicode escape sequence");

                            break;
                        }

                        hex[i] = Read(_token);
                    }

                    // Did we read the full scalar number?
                    if (_reader.Position != codePos + hex.Length)
                        break;

                    var scalar = int.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

                    if (!Rune.TryCreate(scalar, out var rune))
                    {
                        Error(
                            chPos,
                            @"\u".Length + hex.Length,
                            StandardDiagnosticCodes.InvalidUnicodeEscapeSequence,
                            $"Invalid Unicode escape sequence");

                        break;
                    }

                    _string.AddRange(code[..rune.EncodeToUtf16(code)]);

                    break;
                default:
                    Error(chPos, StandardDiagnosticCodes.IncompleteEscapeSequence, "Incomplete escape sequence");
                    break;
            }

            if (replacement is not { } repCh)
                continue;

            Advance(_token);

            _string.Add(repCh);
        }

        return SyntaxTokenKind.StringLiteral;
    }

    private SyntaxTokenKind LexRawStringLiteral(int position)
    {
        int ReadQuotes(int max)
        {
            var i = 0;

            for (; Peek1() == '"' && i < max; i++)
                Advance(_token);

            return i;
        }

        void ReadWhiteSpace(StringBuilder? builder, StringBuilder? indentation)
        {
            while (Peek1() is { } ch && TextFacts.IsWhiteSpace(ch))
            {
                Advance(builder);

                _ = indentation?.Append(ch);
            }
        }

        var openQuotes = ReadQuotes(int.MaxValue);
        var afterOpenQuotes = _reader.Position - position;

        ReadWhiteSpace(_token, indentation: null);

        // Is it a verbatim (i.e. single-line) string literal?
        if (Peek1() is not { } ch1 || !TextFacts.IsNewLine(ch1))
        {
            while (true)
            {
                if (Peek1() is not { } ch2 || TextFacts.IsNewLine(ch2))
                {
                    Error(
                        position,
                        StandardDiagnosticCodes.IncompleteVerbatimStringLiteral,
                        "Incomplete verbatim string literal");

                    break;
                }

                if (ch2 != '"')
                {
                    Advance(_token);

                    continue;
                }

                var beforeCloseQuotes = _reader.Position - position;
                var closeQuotes = ReadQuotes(openQuotes);

                // If we have fewer quotes than the opening quotes, we are still lexing the string contents.
                if (closeQuotes != openQuotes)
                    continue;

                for (var i = afterOpenQuotes; i < beforeCloseQuotes; i++)
                    _string.Add(_token[i]);

                break;
            }

            return SyntaxTokenKind.StringLiteral;
        }

        // It must be a block (i.e. multi-line) string literal.

        if ((Read(_token), Peek1()) == ('\r', '\n'))
            Advance(_token);

        var afterOpener = _reader.Save();
        var lines = 0;

        // First determine the end of the block string literal, along with the indentation string on the closing line.
        while (true)
        {
            _ = _closingIndentation.Clear();

            ReadWhiteSpace(_token, _closingIndentation);

            var closeQuotes = ReadQuotes(openQuotes);

            // Did this quote sequence end the string?
            if (closeQuotes == openQuotes)
                break;

            bool more;

            while (true)
            {
                if (Peek1() is not { } ch2)
                {
                    Error(
                        position,
                        StandardDiagnosticCodes.IncompleteBlockStringLiteral,
                        "Incomplete block string literal");

                    more = false;

                    break;
                }

                if (TextFacts.IsNewLine(ch2))
                {
                    if ((Read(_token), Peek1()) == ('\r', '\n'))
                        Advance(_token);

                    more = true;

                    break;
                }

                if (ch2 == '"')
                {
                    var quotePos = _reader.Position;
                    var lineQuotes = ReadQuotes(int.MaxValue);

                    if (lineQuotes == closeQuotes)
                    {
                        Error(
                            quotePos,
                            StandardDiagnosticCodes.ImproperBlockStringLiteral,
                            "Closing quotes for block string literal must not be on a content line");

                        more = false;

                        break;
                    }
                }

                Advance(_token);
            }

            if (!more)
                break;

            lines++;
        }

        if (_errors)
            return SyntaxTokenKind.StringLiteral;

        // We now know the indentation of the closing line. Go over the string contents again and construct the string
        // value, while making sure that all lines have correct indentation.

        var afterString = _reader.Save();

        _reader.Rewind(afterOpener);

        for (var i = 0; i < lines; i++)
        {
            var indentPos = _reader.Position;

            _ = _currentIndentation.Clear();

            ReadWhiteSpace(builder: null, _currentIndentation);

            static bool StartsWith(StringBuilder builder, StringBuilder value)
            {
                if (builder.Length < value.Length)
                    return false;

                for (var i = 0; i < value.Length; i++)
                    if (builder[i] != value[i])
                        return false;

                return true;
            }

            // We allow the current indentation to only be a prefix of the closing indentation if the current line has
            // no content beyond the indentation.
            if (!StartsWith(_currentIndentation, _closingIndentation) &&
                !(TextFacts.IsNewLine((char)Peek1()!) && StartsWith(_closingIndentation, _currentIndentation)))
            {
                Error(
                    indentPos,
                    StandardDiagnosticCodes.ImproperBlockStringLiteral,
                    "Invalid indentation in block string literal");

                break;
            }

            // Add any white space past the indentation string.
            for (var j = _closingIndentation.Length; j < _currentIndentation.Length; i++)
                _string.Add(_currentIndentation[j]);

            // Add the line content, if any.
            while (!TextFacts.IsNewLine((char)Peek1()!))
                _string.Add(Read(builder: null));

            if ((Read(builder: null), Peek1()) == ('\r', '\n'))
                Advance(builder: null);

            // Finally, add a line break. Block string literals normalize to LF.
            _string.Add('\n');
        }

        _reader.Rewind(afterString);

        return SyntaxTokenKind.StringLiteral;
    }
}
