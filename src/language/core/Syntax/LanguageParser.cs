using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax;

internal sealed class LanguageParser
{
    private readonly SourceText _text;

    private readonly ListReader<SyntaxToken> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly SourceLocation _eoi;

    public LanguageParser(
        SourceText text,
        IReadOnlyList<SyntaxToken> tokens,
        SyntaxMode mode,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _text = text;
        _reader = new(tokens);
        _mode = mode;
        _diagnostics = diagnostics;
        _eoi = (SourceLocation)GetLocation(tokens[^1])!;
    }

    private SyntaxToken? Peek1()
    {
        _ = _reader.TryPeek(0, out var tok);

        return tok;
    }

    private (SyntaxToken? First, SyntaxToken? Second) Peek2()
    {
        _ = _reader.TryPeek(0, out var tok1);
        _ = _reader.TryPeek(1, out var tok2);

        return (tok1, tok2);
    }

    private (SyntaxToken? First, SyntaxToken? Second, SyntaxToken? Third) Peek3()
    {
        _ = _reader.TryPeek(0, out var tok1);
        _ = _reader.TryPeek(1, out var tok2);
        _ = _reader.TryPeek(2, out var tok3);

        return (tok1, tok2, tok3);
    }

    private SyntaxToken Read()
    {
        return _reader.Read();
    }

    private SyntaxToken ExpectCodeIdentifier()
    {
        var next = Peek1();

        // In the vast majority of places where we use lowercase identifiers in the language, there is no reason to
        // reserve these common words for use as type keywords. So allow them as lowercase identifiers.
        if (next?.Kind is { } kind && SyntaxFacts.IsCodeIdentifier(kind))
            return Read();

        ErrorExpected(StandardDiagnosticCodes.ExpectedToken, GetLocation(next), "lowercase identifier");

        return Missing();
    }

    private SyntaxToken ExpectBindingIdentifier()
    {
        var next = Peek1();

        // Same idea as above.
        if (next?.Kind is { } kind && SyntaxFacts.IsBindingIdentifier(kind))
            return Read();

        ErrorExpected(StandardDiagnosticCodes.ExpectedToken, GetLocation(next), "lowercase or discard identifier");

        return Missing();
    }

    private SyntaxToken ExpectLiteral()
    {
        var next = Peek1();

        if (next?.Kind is
            SyntaxTokenKind.NilLiteral or
            SyntaxTokenKind.BooleanLiteral or
            SyntaxTokenKind.IntegerLiteral or
            SyntaxTokenKind.RealLiteral or
            SyntaxTokenKind.AtomLiteral or
            SyntaxTokenKind.StringLiteral)
            return Read();

        ErrorExpected(StandardDiagnosticCodes.ExpectedToken, GetLocation(next), "literal");

        return Missing();
    }

    private SyntaxToken ExpectNumericLiteral()
    {
        return Expect(SyntaxTokenKind.IntegerLiteral, SyntaxTokenKind.RealLiteral);
    }

    private SyntaxToken Expect(SyntaxTokenKind kind1, SyntaxTokenKind kind2)
    {
        var next = Peek1();
        var kind = next?.Kind;

        if (kind == kind1 || kind == kind2)
            return Read();

        ErrorExpected(
            StandardDiagnosticCodes.ExpectedToken,
            GetLocation(next),
            $"{SyntaxFacts.GetFriendlyName(kind1)} or {SyntaxFacts.GetFriendlyName(kind2)}");

        return Missing();
    }

    private SyntaxToken Expect(SyntaxTokenKind kind)
    {
        var next = Peek1();

        if (next?.Kind == kind)
            return Read();

        ErrorExpected(StandardDiagnosticCodes.ExpectedToken, GetLocation(next), SyntaxFacts.GetFriendlyName(kind));

        return Missing();
    }

    private SyntaxToken? OptionalMinus()
    {
        return IsMinus(Peek1()) ? Read() : null;
    }

    private static bool IsMinus(SyntaxToken? token)
    {
        return token is { Kind: SyntaxTokenKind.AdditiveOperator, Text: "-" };
    }

    private SyntaxToken? Optional(SyntaxTokenKind kind)
    {
        // TODO: Take params ReadOnlySpan<SyntaxTokenKind>.
        return Peek1()?.Kind == kind ? Read() : null;
    }

    private static SyntaxToken Missing()
    {
        return new();
    }

    private void ErrorExpected(SourceDiagnosticCode code, SourceLocation? location, string expected)
    {
        _diagnostics.Add(
            SourceDiagnostic.Create(
                code, SourceDiagnosticSeverity.Error, location ?? _eoi, $"Expected {expected}"));
    }

    private SourceLocation? GetLocation(SyntaxItem? item)
    {
        // We cannot use the convenience methods on SyntaxItem during parsing as the Analysis property is unusable.
        return item != null ? _text.GetLocation(item.Span) : null;
    }

    private static ImmutableArray<T>.Builder Builder<T>()
        where T : SyntaxItem
    {
        return ImmutableArray.CreateBuilder<T>();
    }

    private static (ImmutableArray<T>.Builder Elements, ImmutableArray<SyntaxToken>.Builder Separators)
        SeparatedBuilder<T>()
        where T : SyntaxItem
    {
        return (Builder<T>(), Builder<SyntaxToken>());
    }

    private static SyntaxItemList<T> List<T>(ImmutableArray<T>.Builder elements)
        where T : SyntaxItem
    {
        return new(elements.DrainToImmutable());
    }

    private static SeparatedSyntaxItemList<TElement, SyntaxToken> List<TElement>(
        ImmutableArray<TElement>.Builder elements, ImmutableArray<SyntaxToken>.Builder separators)
        where TElement : SyntaxItem
    {
        return new(List(elements), List(separators));
    }

    private T? ParseOptional<T>(SyntaxTokenKind kind, Func<LanguageParser, T> parser)
        where T : SyntaxNode
    {
        return Peek1()?.Kind == kind ? parser(this) : null;
    }

    private (ImmutableArray<T>.Builder Elements, ImmutableArray<SyntaxToken>.Builder Separators) ParseSeparatedList<T>(
        Func<LanguageParser, T> parser,
        SyntaxTokenKind separator,
        SyntaxTokenKind closer,
        bool allowEmpty,
        bool allowTrailing)
        where T : SyntaxNode
    {
        // TODO: The way we parse a parameter list (and other similar syntax nodes) causes the parser to misinterpret
        // the entire function body for some invalid inputs. We need to do better here.

        var result = SeparatedBuilder<T>();
        var (elems, seps) = result;

        bool NextIsRelevant()
        {
            return Peek1() is { IsEndOfInput: false } next && next.Kind != closer;
        }

        if (!allowTrailing)
        {
            if (allowEmpty && !NextIsRelevant())
                return result;

            elems.Add(parser(this));

            while (Optional(separator) is { } sep)
            {
                seps.Add(sep);
                elems.Add(parser(this));
            }

            return result;
        }

        if (!allowEmpty)
        {
            elems.Add(parser(this));

            if (Optional(separator) is not { } sep)
                return result;

            seps.Add(sep);
        }

        while (NextIsRelevant())
        {
            elems.Add(parser(this));

            if (Optional(separator) is not { } sep2)
                break;

            seps.Add(sep2);
        }

        return result;
    }

    // Documents

    public DocumentSyntax ParseDocument()
    {
        return _mode switch
        {
            SyntaxMode.Module => ParseModuleDocument(),
            SyntaxMode.Interactive => ParseInteractiveDocument(),
            _ => throw new UnreachableException(),
        };
    }

    private ModuleDocumentSyntax ParseModuleDocument()
    {
        var mattrs = ParseAttributes();
        var mod = Expect(SyntaxTokenKind.ModKeyword);
        var path = ParseModulePath();
        var semi = Expect(SyntaxTokenKind.Semicolon);
        var decls = Builder<DeclarationSyntax>();

        while (Peek1() is { IsEndOfInput: false })
        {
            var dattrs = ParseAttributes();
            var skipped = Builder<SyntaxToken>();

            void DrainToMissingDeclaration(bool attributes)
            {
                if (skipped.Count == 0 && (!attributes || dattrs.Count == 0))
                    return;

                ErrorExpected(
                    StandardDiagnosticCodes.MissingDeclaration,
                    GetLocation(skipped.FirstOrDefault() ?? Peek1()),
                    "declaration");

                decls.Add(new MissingDeclarationSyntax(List(dattrs), List(skipped), Missing()));
            }

            while (Peek1() is { IsEndOfInput: false } next)
            {
                if (SyntaxFacts.IsDeclarationStarter(next.Kind))
                {
                    DrainToMissingDeclaration(false);

                    decls.Add(ParseDeclaration(dattrs));

                    break;
                }

                skipped.Add(Read());
            }

            // There can be some leftover skipped tokens.
            DrainToMissingDeclaration(true);
        }

        var eoi = Expect(SyntaxTokenKind.EndOfInput);

        return new(List(mattrs), mod, path, semi, List(decls), eoi);
    }

    private InteractiveDocumentSyntax ParseInteractiveDocument()
    {
        var subs = Builder<SubmissionSyntax>();

        while (Peek1() is { IsEndOfInput: false })
        {
            var attrs = ParseAttributes();
            var skipped = Builder<SyntaxToken>();

            void DrainToMissingStatement(bool attributes)
            {
                if (skipped.Count == 0 && (!attributes || attrs.Count == 0))
                    return;

                ErrorExpected(
                    StandardDiagnosticCodes.MissingStatement,
                    GetLocation(skipped.FirstOrDefault() ?? Peek1()),
                    "interactive declaration or statement");

                subs.Add(
                    new StatementSubmissionSyntax(new MissingStatementSyntax(List(attrs), List(skipped), Missing())));
            }

            while (Peek1() is { IsEndOfInput: false } next)
            {
                if (SyntaxFacts.IsInteractiveDeclarationStarter(next.Kind))
                {
                    DrainToMissingStatement(false);

                    subs.Add(new DeclarationSubmissionSyntax(ParseDeclaration(attrs)));

                    break;
                }

                if (SyntaxFacts.IsInteractiveStatementStarter(next.Kind))
                {
                    DrainToMissingStatement(false);

                    subs.Add(new StatementSubmissionSyntax(ParseStatement(attrs)));

                    break;
                }

                skipped.Add(Read());

                if (next.Kind == SyntaxTokenKind.Semicolon)
                    break;
            }

            // There can be some leftover skipped tokens.
            DrainToMissingStatement(true);
        }

        var eoi = Expect(SyntaxTokenKind.EndOfInput);

        return new(List(subs), eoi);
    }

    // Miscellaneous

    private ImmutableArray<AttributeSyntax>.Builder ParseAttributes()
    {
        var attrs = Builder<AttributeSyntax>();

        while (Peek1()?.Kind == SyntaxTokenKind.At)
            attrs.Add(ParseAttribute());

        return attrs;
    }

    private AttributeSyntax ParseAttribute()
    {
        var at = Read();
        var name = ExpectCodeIdentifier();
        var value = ExpectLiteral();

        return new(at, name, value);
    }

    private ModulePathSyntax ParseModulePath()
    {
        var (idents, seps) = SeparatedBuilder<SyntaxToken>();

        idents.Add(Expect(SyntaxTokenKind.UpperIdentifier));

        while (Optional(SyntaxTokenKind.ColonColon) is { } sep)
        {
            seps.Add(sep);
            idents.Add(Expect(SyntaxTokenKind.UpperIdentifier));
        }

        return new(List(idents, seps));
    }

    // Declarations

    private DeclarationSyntax ParseDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var (tok1, tok2) = Peek2()!;

        return (tok1?.Kind, tok2?.Kind) switch
        {
            (SyntaxTokenKind.UseKeyword, _) => ParseUseDeclaration(attributes),
            (SyntaxTokenKind.TypeKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.OpaqueKeyword or SyntaxTokenKind.TypeKeyword) =>
                ParseTypeDeclaration(attributes),
            (SyntaxTokenKind.ConstKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.ConstKeyword) => ParseConstantDeclaration(attributes),
            (SyntaxTokenKind.FnKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.ExtKeyword or SyntaxTokenKind.FnKeyword) =>
                ParseFunctionDeclaration(attributes),
            (SyntaxTokenKind.TestKeyword, _) => ParseTestDeclaration(attributes),
            _ => throw new UnreachableException(),
        };
    }

    private UseDeclarationSyntax ParseUseDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var use = Read();
        var name = Expect(SyntaxTokenKind.UpperIdentifier);
        var equals = Expect(SyntaxTokenKind.Equals);
        var path = ParseModulePath();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), use, name, equals, path, semi);
    }

    private TypeDeclarationSyntax ParseTypeDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var pub = Optional(SyntaxTokenKind.PubKeyword);
        var opaque = pub != null ? Optional(SyntaxTokenKind.OpaqueKeyword) : null;
        var kw = Expect(SyntaxTokenKind.TypeKeyword);
        var name = Expect(SyntaxTokenKind.LowerIdentifier);
        var parms = ParseOptional(SyntaxTokenKind.OpenParen, static @this => @this.ParseTypeParameterList());
        var equals = Expect(SyntaxTokenKind.Equals);
        var type = ParseType();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), pub, opaque, kw, name, parms, equals, type, semi);
    }

    private TypeParameterListSyntax ParseTypeParameterList()
    {
        var open = Read();
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseTypeParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: false,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private TypeParameterSyntax ParseTypeParameter()
    {
        var attrs = ParseAttributes();
        var name = Expect(SyntaxTokenKind.LowerIdentifier, SyntaxTokenKind.DiscardIdentifier);

        return new(List(attrs), name);
    }

    private ConstantDeclarationSyntax ParseConstantDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var pub = Optional(SyntaxTokenKind.PubKeyword);
        var @const = Expect(SyntaxTokenKind.ConstKeyword);
        var name = ExpectCodeIdentifier();
        var type = ParseOptional(SyntaxTokenKind.Colon, static @this => @this.ParseTypeAnnotation());
        var equals = Expect(SyntaxTokenKind.Equals);
        var body = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), pub, @const, name, type, equals, body, semi);
    }

    private FunctionDeclarationSyntax ParseFunctionDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var pub = Optional(SyntaxTokenKind.PubKeyword);
        var ext = Optional(SyntaxTokenKind.ExtKeyword);
        var fn = Expect(SyntaxTokenKind.FnKeyword);
        var name = ExpectCodeIdentifier();
        var parms = ParseFunctionParameterList();
        var err = Optional(SyntaxTokenKind.ErrKeyword);
        var type = ParseOptional(SyntaxTokenKind.MinusCloseAngle, static @this => @this.ParseReturnTypeAnnotation());
        var body = ext == null ? ParseBlockExpression() : null;

        return new(List(attributes), pub, ext, fn, name, parms, err, type, body);
    }

    private FunctionParameterListSyntax ParseFunctionParameterList()
    {
        var open = Read();
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseFunctionParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private FunctionParameterSyntax ParseFunctionParameter()
    {
        var attrs = ParseAttributes();
        var name = ExpectBindingIdentifier();
        var type = ParseOptional(SyntaxTokenKind.Colon, static @this => @this.ParseTypeAnnotation());

        return new(List(attrs), name, type);
    }

    private TestDeclarationSyntax ParseTestDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var test = Read();
        var name = ExpectCodeIdentifier();
        var body = ParseBlockExpression();

        return new(List(attributes), test, name, body);
    }

    // Types

    private TypeSyntax ParseType()
    {
        var type = ParsePrimaryType();

        if (Optional(SyntaxTokenKind.OrKeyword) is { } or)
        {
            var (types, seps) = SeparatedBuilder<TypeSyntax>();

            types.Add(type);
            seps.Add(or);
            types.Add(ParsePrimaryType());

            while (Optional(SyntaxTokenKind.OrKeyword) is { } sep)
            {
                seps.Add(sep);
                types.Add(ParsePrimaryType());
            }

            return new UnionTypeSyntax(List(types, seps));
        }

        return type;
    }

    private TypeSyntax ParsePrimaryType()
    {
        var (tok1, tok2, tok3) = Peek3();
        var type = (tok1?.Kind, tok2?.Kind, tok3?.Kind) switch
        {
            (SyntaxTokenKind.AnyKeyword, _, _) => ParseAnyType(),
            _ when IsMinus(tok1) => ParseLiteralType(),
            ({ } literal, _, _) when SyntaxFacts.IsLiteral(literal) => ParseLiteralType(),
            (SyntaxTokenKind.BoolKeyword, _, _) => ParseBooleanType(),
            (SyntaxTokenKind.IntKeyword, _, _) => ParseIntegerType(),
            (SyntaxTokenKind.RealKeyword, _, _) => ParseRealType(),
            (SyntaxTokenKind.AtomKeyword, _, _) => ParseAtomType(),
            (SyntaxTokenKind.StrKeyword, _, _) => ParseStringType(),
            (SyntaxTokenKind.RefKeyword, _, _) => ParseReferenceType(),
            (SyntaxTokenKind.HandleKeyword, _, _) => ParseHandleType(),
            (SyntaxTokenKind.ModKeyword, _, _) => ParseModuleType(),
            (SyntaxTokenKind.RecKeyword, _, _) => ParseRecordType(),
            (SyntaxTokenKind.ErrKeyword, _, _) => ParseErrorType(),
            (SyntaxTokenKind.OpenParen, _, _) => ParseTupleType(),
            (SyntaxTokenKind.OpenBracket, _, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.OpenBracket, _) => ParseArrayType(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace) => ParseSetType(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket) => ParseMapType(),
            (SyntaxTokenKind.FnKeyword, _, _) => ParseFunctionType(),
            (SyntaxTokenKind.AgentKeyword, _, _) => ParseAgentType(),
            (SyntaxTokenKind.UpperIdentifier or SyntaxTokenKind.LowerIdentifier, _, _) => ParseNominalType(),
            _ => default(TypeSyntax),
        };

        if (type == null)
        {
            ErrorExpected(StandardDiagnosticCodes.MissingType, GetLocation(Peek1()), "type");

            type = new NominalTypeSyntax(null, Missing(), null);
        }

        return type;
    }

    private AnyTypeSyntax ParseAnyType()
    {
        var any = Read();

        return new(any);
    }

    private LiteralTypeSyntax ParseLiteralType()
    {
        var minus = OptionalMinus();
        var literal = minus != null ? ExpectNumericLiteral() : Read();

        return new(minus, literal);
    }

    private BooleanTypeSyntax ParseBooleanType()
    {
        var @bool = Read();

        return new(@bool);
    }

    private IntegerTypeSyntax ParseIntegerType()
    {
        var @int = Read();
        var range = ParseOptional(SyntaxTokenKind.OpenParen, static @this => @this.ParseIntegerTypeRange());

        return new(@int, range);
    }

    private IntegerTypeRangeSyntax ParseIntegerTypeRange()
    {
        var open = Read();
        var lower = default(IntegerTypeRangeBoundSyntax);

        static bool IsBoundStarter(SyntaxToken? token)
        {
            return IsMinus(token) || token?.Kind == SyntaxTokenKind.IntegerLiteral;
        }

        if (Peek1() is { } nextLower && IsBoundStarter(nextLower))
            lower = ParseIntegerTypeRangeBound();

        var sep = Expect(SyntaxTokenKind.DotDot);
        var upper = default(IntegerTypeRangeBoundSyntax);

        if (lower == null || (Peek1() is { } nextUpper && IsBoundStarter(nextUpper)))
            upper = ParseIntegerTypeRangeBound();

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, lower, sep, upper, close);
    }

    private IntegerTypeRangeBoundSyntax ParseIntegerTypeRangeBound()
    {
        var minus = OptionalMinus();
        var literal = minus != null ? Expect(SyntaxTokenKind.IntegerLiteral) : Read();

        return new(minus, literal);
    }

    private RealTypeSyntax ParseRealType()
    {
        var real = Read();

        return new(real);
    }

    private AtomTypeSyntax ParseAtomType()
    {
        var atom = Read();

        return new(atom);
    }

    private StringTypeSyntax ParseStringType()
    {
        var str = Read();

        return new(str);
    }

    private ReferenceTypeSyntax ParseReferenceType()
    {
        var @ref = Read();

        return new(@ref);
    }

    private HandleTypeSyntax ParseHandleType()
    {
        var handle = Read();

        return new(handle);
    }

    private ModuleTypeSyntax ParseModuleType()
    {
        var mod = Read();

        return new(mod);
    }

    private RecordTypeSyntax ParseRecordType()
    {
        var rec = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregateTypeField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(rec, open, List(fields, seps), close);
    }

    private ErrorTypeSyntax ParseErrorType()
    {
        var err = Read();
        var name = Optional(SyntaxTokenKind.UpperIdentifier);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregateTypeField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(err, name, open, List(fields, seps), close);
    }

    private AggregateTypeFieldSyntax ParseAggregateTypeField()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var name = ExpectCodeIdentifier();
        var colon = Expect(SyntaxTokenKind.Colon);
        var type = ParseType();

        return new(mut, name, colon, type);
    }

    private TupleTypeSyntax ParseTupleType()
    {
        var open = Read();
        var (comps, seps) = SeparatedBuilder<TypeSyntax>();

        // TODO: Can we merge this with ParseSeparatedList?
        comps.Add(ParseType());
        seps.Add(Expect(SyntaxTokenKind.Comma));
        comps.Add(ParseType());

        while (Optional(SyntaxTokenKind.Comma) is { } sep)
        {
            seps.Add(sep);
            comps.Add(ParseType());
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(comps, seps), close);
    }

    private ArrayTypeSyntax ParseArrayType()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var open = Read();
        var elem = ParseType();
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(mut, open, elem, close);
    }

    private SetTypeSyntax ParseSetType()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var elem = ParseType();
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(mut, hash, open, elem, close);
    }

    private MapTypeSyntax ParseMapType()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var (pairs, seps) = ParseSeparatedList(
            static @this => @this.ParseMapTypePair(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(mut, hash, open, List(pairs, seps), close);
    }

    private MapTypePairSyntax ParseMapTypePair()
    {
        var key = ParseType();
        var colon = Expect(SyntaxTokenKind.Colon);
        var question = Optional(SyntaxTokenKind.Question);
        var value = ParseType();

        return new(key, colon, question, value);
    }

    private FunctionTypeSyntax ParseFunctionType()
    {
        var fn = Read();
        var sig = ParseOptional(SyntaxTokenKind.OpenParen, static @this => @this.ParseFunctionTypeSignature());

        return new(fn, sig);
    }

    private FunctionTypeSignatureSyntax ParseFunctionTypeSignature()
    {
        var open = Read();
        var parms = ParseFunctionTypeParameterList();
        var err = Optional(SyntaxTokenKind.ErrKeyword);
        var type = ParseReturnTypeAnnotation();
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, parms, err, type, close);
    }

    private FunctionTypeParameterListSyntax ParseFunctionTypeParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseFunctionTypeParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private FunctionTypeParameterSyntax ParseFunctionTypeParameter()
    {
        var attrs = ParseAttributes();
        var type = ParseType();

        return new(List(attrs), type);
    }

    private AgentTypeSyntax ParseAgentType()
    {
        var agent = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (msgs, seps) = ParseSeparatedList(
            static @this => @this.ParseAgentTypeMessage(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(agent, open, List(msgs, seps), close);
    }

    private AgentTypeMessageSyntax ParseAgentTypeMessage()
    {
        var name = ExpectCodeIdentifier();
        var parms = ParseAgentTypeMessageParameterList();

        return new(name, parms);
    }

    private AgentTypeMessageParameterListSyntax ParseAgentTypeMessageParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseAgentTypeMessageParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private AgentTypeMessageParameterSyntax ParseAgentTypeMessageParameter()
    {
        var type = ParseType();

        return new(type);
    }

    private NominalTypeSyntax ParseNominalType()
    {
        var path = ParseOptional(SyntaxTokenKind.UpperIdentifier, static @this => @this.ParseNominalTypePath());
        var name = Expect(SyntaxTokenKind.LowerIdentifier);
        var args = ParseOptional(SyntaxTokenKind.OpenParen, static @this => @this.ParseNominalTypeArgumentList());

        return new(path, name, args);
    }

    private NominalTypePathSyntax ParseNominalTypePath()
    {
        var path = ParseModulePath();
        var dot = Expect(SyntaxTokenKind.Dot);

        return new(path, dot);
    }

    private NominalTypeArgumentListSyntax ParseNominalTypeArgumentList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (args, seps) = ParseSeparatedList(
            static @this => @this.ParseType(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: false,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(args, seps), close);
    }

    private ReturnTypeSyntax ParseReturnType()
    {
        return Peek1()?.Kind switch
        {
            SyntaxTokenKind.NoneKeyword => ParseNoneReturnType(),
            _ => ParseNormalReturnType(),
        };
    }

    private NormalReturnTypeSyntax ParseNormalReturnType()
    {
        var type = ParseType();

        return new(type);
    }

    private NoneReturnTypeSyntax ParseNoneReturnType()
    {
        var none = Read();

        return new(none);
    }

    private TypeAnnotationSyntax ParseTypeAnnotation()
    {
        var colon = Read();
        var type = ParseType();

        return new(colon, type);
    }

    private ReturnTypeAnnotationSyntax ParseReturnTypeAnnotation()
    {
        var arrow = Read();
        var type = ParseReturnType();

        return new(arrow, type);
    }

    // Statements

    private StatementSyntax ParseStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        return Peek1()?.Kind switch
        {
            SyntaxTokenKind.LetKeyword => ParseLetStatement(attributes),
            SyntaxTokenKind.DeferKeyword => ParseDeferStatement(attributes),
            SyntaxTokenKind.AssertKeyword => ParseAssertStatement(attributes),
            _ => ParseExpressionStatement(attributes),
        };
    }

    private AssertStatementSyntax ParseAssertStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var assert = Read();
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), assert, expr, semi);
    }

    private DeferStatementSyntax ParseDeferStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var defer = Read();
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), defer, expr, semi);
    }

    private LetStatementSyntax ParseLetStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var let = Read();
        var pat = ParsePattern();
        var equals = Expect(SyntaxTokenKind.Equals);
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), let, pat, equals, expr, semi);
    }

    private ExpressionStatementSyntax ParseExpressionStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), expr, semi);
    }

    // Expressions

    private ExpressionSyntax ParseExpression()
    {
        return ParseAssignmentExpression();
    }

    private ExpressionSyntax ParseAssignmentExpression()
    {
        var expr = ParseLogicalExpression();

        while (Optional(SyntaxTokenKind.Equals) is { } op)
        {
            var right = ParseAssignmentExpression();

            expr = new AssignmentExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseLogicalExpression()
    {
        var expr = ParseRelationalExpression();

        while (Peek1()?.Kind is SyntaxTokenKind.AndKeyword or SyntaxTokenKind.OrKeyword)
        {
            var op = Read();
            var right = ParseRelationalExpression();

            expr = new LogicalExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseRelationalExpression()
    {
        var expr = ParseBitwiseExpression();

        while (Peek1()?.Kind is
            SyntaxTokenKind.ExclamationEquals or
            SyntaxTokenKind.EqualsEquals or
            SyntaxTokenKind.CloseAngle or
            SyntaxTokenKind.CloseAngleEquals or
            SyntaxTokenKind.OpenAngle or
            SyntaxTokenKind.OpenAngleEquals)
        {
            var op = Read();
            var right = ParseBitwiseExpression();

            expr = new RelationalExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseBitwiseExpression()
    {
        var expr = ParseShiftExpression();

        while (Optional(SyntaxTokenKind.BitwiseOperator) is { } op)
        {
            var right = ParseShiftExpression();

            expr = new BitwiseExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseShiftExpression()
    {
        var expr = ParseAdditiveExpression();

        while (Optional(SyntaxTokenKind.ShiftOperator) is { } op)
        {
            var right = ParseAdditiveExpression();

            expr = new ShiftExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseAdditiveExpression()
    {
        var expr = ParseMultiplicativeExpression();

        while (Optional(SyntaxTokenKind.AdditiveOperator) is { } op)
        {
            var right = ParseMultiplicativeExpression();

            expr = new AdditiveExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseMultiplicativeExpression()
    {
        var expr = ParsePrefixExpression();

        while (Optional(SyntaxTokenKind.MultiplicativeOperator) is { } op)
        {
            var right = ParsePrefixExpression();

            expr = new MultiplicativeExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParsePrefixExpression()
    {
        return Peek1()?.Kind switch
        {
            SyntaxTokenKind.BitwiseOperator or
            SyntaxTokenKind.ShiftOperator or
            SyntaxTokenKind.MultiplicativeOperator or
            SyntaxTokenKind.AdditiveOperator or
            SyntaxTokenKind.NotKeyword => ParseUnaryExpression(),
            _ => ParsePrimaryExpression(),
        };
    }

    private UnaryExpressionSyntax ParseUnaryExpression()
    {
        var op = Read();
        var oper = ParsePrefixExpression();

        return new(op, oper);
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        var (tok1, tok2, tok3) = Peek3();
        var expr = (tok1?.Kind, tok2?.Kind, tok3?.Kind) switch
        {
            (SyntaxTokenKind.OpenParen, _, _) => ParseParenthesizedOrTupleExpression(),
            (SyntaxTokenKind.OpenBrace, _, _) => ParseBlockExpression(),
            ({ } ident, _, _) when SyntaxFacts.IsBindingIdentifier(ident) => ParseIdentifierExpression(),
            ({ } literal, _, _) when SyntaxFacts.IsLiteral(literal) => ParseLiteralExpression(),
            (SyntaxTokenKind.ThisKeyword, _, _) => ParseThisExpression(),
            (SyntaxTokenKind.UpperIdentifier, _, _) => ParseModuleExpression(),
            (SyntaxTokenKind.RecKeyword, _, _) => ParseRecordExpression(),
            (SyntaxTokenKind.ErrKeyword, _, _) => ParseErrorExpression(),
            (SyntaxTokenKind.OpenBracket, _, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.OpenBracket, _) => ParseArrayExpression(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace) => ParseSetExpression(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket, _) or
            (SyntaxTokenKind.MutKeyword, SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket) => ParseMapExpression(),
            (SyntaxTokenKind.FnKeyword, _, _) => ParseLambdaExpression(),
            (SyntaxTokenKind.IfKeyword, _, _) => ParseIfExpression(),
            (SyntaxTokenKind.CondKeyword, _, _) => ParseConditionExpression(),
            (SyntaxTokenKind.MatchKeyword, _, _) => ParseMatchExpression(),
            (SyntaxTokenKind.RecvKeyword, _, _) => ParseReceiveExpression(),
            (SyntaxTokenKind.TryKeyword, _, _) => ParseTryExpression(),
            (SyntaxTokenKind.WhileKeyword, _, _) => ParseWhileExpression(),
            (SyntaxTokenKind.ForKeyword, _, _) => ParseForExpression(),
            (SyntaxTokenKind.RetKeyword or SyntaxTokenKind.TailKeyword, _, _) => ParseReturnExpression(),
            (SyntaxTokenKind.RaiseKeyword, _, _) => ParseRaiseExpression(),
            (SyntaxTokenKind.NextKeyword, _, _) => ParseNextExpression(),
            (SyntaxTokenKind.BreakKeyword, _, _) => ParseBreakExpression(),
            _ => null,
        };

        if (expr == null)
        {
            ErrorExpected(StandardDiagnosticCodes.MissingExpression, GetLocation(Peek1()), "expression");

            expr = new IdentifierExpressionSyntax(Missing());
        }

        return ParsePostfixExpression(expr);
    }

    private ExpressionSyntax ParseParenthesizedOrTupleExpression()
    {
        var open = Read();
        var expr = ParseExpression();

        if (Optional(SyntaxTokenKind.Comma) is { } comma)
        {
            var (exprs, seps) = SeparatedBuilder<ExpressionSyntax>();

            // TODO: Can we merge this with ParseSeparatedList?
            exprs.Add(expr);
            seps.Add(comma);
            exprs.Add(ParseExpression());

            while (Optional(SyntaxTokenKind.Comma) is { } sep)
            {
                seps.Add(sep);
                exprs.Add(ParseExpression());
            }

            var tclose = Expect(SyntaxTokenKind.CloseParen);

            return new TupleExpressionSyntax(open, List(exprs, seps), tclose);
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new ParenthesizedExpressionSyntax(open, expr, close);
    }

    private BlockExpressionSyntax ParseBlockExpression()
    {
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var stmts = Builder<StatementSyntax>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            var mark = _reader.Save();
            var attrs = ParseAttributes();
            var skipped = Builder<SyntaxToken>();

            void DrainToMissingStatement()
            {
                if (skipped.Count == 0)
                    return;

                ErrorExpected(
                    StandardDiagnosticCodes.MissingStatement,
                    GetLocation(skipped.FirstOrDefault() ?? Peek1()),
                    "statement");

                stmts.Add(new MissingStatementSyntax(List(attrs), List(skipped), Missing()));
            }

            var decl = false;

            while (Peek2() is ({ IsEndOfInput: false } next1, var next2))
            {
                // We might be looking at a declaration because the user has not yet closed the current block
                // expression. If so, stop parsing this block so we can properly parse the declaration.
                //
                // Note this ambiguity:
                //
                // {
                //     fn() {};
                // }
                //
                // The fn keyword could be confused with a declaration. For this case, we need to look ahead.
                if (SyntaxFacts.IsDeclarationStarter(next1.Kind) &&
                    (next1.Kind, next2?.Kind) is not (SyntaxTokenKind.FnKeyword, SyntaxTokenKind.OpenParen))
                {
                    DrainToMissingStatement();

                    // If we have any attributes that were not attached to the MissingStatementSyntax, rewind so that
                    // they become available when the declaration is parsed.
                    if (attrs.Count != 0)
                        _reader.Rewind(mark);

                    decl = true;

                    break;
                }

                if (SyntaxFacts.IsStatementStarter(next1.Kind))
                {
                    DrainToMissingStatement();

                    stmts.Add(ParseStatement(attrs));

                    break;
                }

                skipped.Add(Read());

                if (next1.Kind == SyntaxTokenKind.Semicolon)
                    break;
            }

            // There can be some leftover skipped tokens.
            DrainToMissingStatement();

            if (decl)
                break;
        }

        // Blocks must have at least one statement.
        if (stmts.Count == 0)
        {
            ErrorExpected(StandardDiagnosticCodes.MissingStatement, GetLocation(Peek1()), "statement");

            stmts.Add(new MissingStatementSyntax(
                SyntaxItemList<AttributeSyntax>.Empty, SyntaxItemList<SyntaxToken>.Empty, Missing()));
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(open, List(stmts), close);
    }

    private IdentifierExpressionSyntax ParseIdentifierExpression()
    {
        var name = Read();

        return new(name);
    }

    private ThisExpressionSyntax ParseThisExpression()
    {
        var @this = Read();

        return new(@this);
    }

    private LiteralExpressionSyntax ParseLiteralExpression()
    {
        var literal = Read();

        return new(literal);
    }

    private ModuleExpressionSyntax ParseModuleExpression()
    {
        var path = ParseModulePath();

        return new(path);
    }

    private RecordExpressionSyntax ParseRecordExpression()
    {
        var rec = Read();
        var with = ParseOptional(SyntaxTokenKind.WithKeyword, static @this => @this.ParseAggregateExpressionWith());
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregateExpressionField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(rec, with, open, List(fields, seps), close);
    }

    private ErrorExpressionSyntax ParseErrorExpression()
    {
        var err = Read();
        var name = Expect(SyntaxTokenKind.UpperIdentifier);
        var with = ParseOptional(SyntaxTokenKind.WithKeyword, static @this => @this.ParseAggregateExpressionWith());
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregateExpressionField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(err, name, with, open, List(fields, seps), close);
    }

    private AggregateExpressionWithSyntax ParseAggregateExpressionWith()
    {
        var with = Read();
        var operand = ParseExpression();

        return new(with, operand);
    }

    private AggregateExpressionFieldSyntax ParseAggregateExpressionField()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var name = ExpectCodeIdentifier();
        var colon = Expect(SyntaxTokenKind.Colon);
        var value = ParseExpression();

        return new(mut, name, colon, value);
    }

    private ArrayExpressionSyntax ParseArrayExpression()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var open = Read();
        var (elems, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(mut, open, List(elems, seps), close);
    }

    private SetExpressionSyntax ParseSetExpression()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var (elems, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(mut, hash, open, List(elems, seps), close);
    }

    private MapExpressionSyntax ParseMapExpression()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var (pairs, seps) = ParseSeparatedList(
            static @this => @this.ParseMapExpressionPair(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(mut, hash, open, List(pairs, seps), close);
    }

    private MapExpressionPairSyntax ParseMapExpressionPair()
    {
        var key = ParseExpression();
        var colon = Expect(SyntaxTokenKind.Colon);
        var value = ParseExpression();

        return new(key, colon, value);
    }

    private LambdaExpressionSyntax ParseLambdaExpression()
    {
        var fn = Read();
        var parms = ParseLambdaParameterList();
        var arrow = Expect(SyntaxTokenKind.MinusCloseAngle);
        var body = ParseExpression();

        return new(fn, parms, arrow, body);
    }

    private LambdaParameterListSyntax ParseLambdaParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseLambdaParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private LambdaParameterSyntax ParseLambdaParameter()
    {
        var attrs = ParseAttributes();
        var name = ExpectBindingIdentifier();

        return new(List(attrs), name);
    }

    private IfExpressionSyntax ParseIfExpression()
    {
        var @if = Read();
        var condition = ParseExpression();
        var body = ParseBlockExpression();
        var @else = ParseOptional(SyntaxTokenKind.ElseKeyword, static @this => @this.ParseExpressionElse());

        return new(@if, condition, body, @else);
    }

    private ExpressionElseSyntax ParseExpressionElse()
    {
        var @else = Read();
        var body = ParseBlockExpression();

        return new(@else, body);
    }

    private ConditionExpressionSyntax ParseConditionExpression()
    {
        var cond = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (arms, seps) = ParseSeparatedList(
            static @this => @this.ParseConditionExpressionArm(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: false,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(cond, open, List(arms, seps), close);
    }

    private ConditionExpressionArmSyntax ParseConditionExpressionArm()
    {
        var condition = ParseExpression();
        var arrow = Expect(SyntaxTokenKind.MinusCloseAngle);
        var body = ParseExpression();

        return new(condition, arrow, body);
    }

    private MatchExpressionSyntax ParseMatchExpression()
    {
        var match = Read();
        var oper = ParseExpression();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (arms, seps) = ParseSeparatedList(
            static @this => @this.ParseExpressionPatternArm(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: false,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(match, oper, open, List(arms, seps), close);
    }

    private ExpressionPatternArmSyntax ParseExpressionPatternArm()
    {
        var pat = ParsePattern();
        var guard = ParseOptional(SyntaxTokenKind.IfKeyword, static @this => @this.ParseExpressionArmGuard());
        var arrow = Expect(SyntaxTokenKind.MinusCloseAngle);
        var body = ParseExpression();

        return new(pat, guard, arrow, body);
    }

    private ExpressionArmGuardSyntax ParseExpressionArmGuard()
    {
        var @if = Read();
        var condition = ParseExpression();

        return new(@if, condition);
    }

    private ReceiveExpressionSyntax ParseReceiveExpression()
    {
        var recv = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (arms, seps) = ParseSeparatedList(
            static @this => @this.ParseReceiveExpressionArm(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: false,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);
        var @else = ParseOptional(SyntaxTokenKind.ElseKeyword, static @this => @this.ParseExpressionElse());

        return new(recv, open, List(arms, seps), close, @else);
    }

    private ReceiveExpressionArmSyntax ParseReceiveExpressionArm()
    {
        var name = ExpectCodeIdentifier();
        var parms = ParseReceiveParameterList();
        var guard = ParseOptional(SyntaxTokenKind.IfKeyword, static @this => @this.ParseExpressionArmGuard());
        var arrow = Expect(SyntaxTokenKind.MinusCloseAngle);
        var body = ParseExpression();

        return new(name, parms, guard, arrow, body);
    }

    private ReceiveParameterListSyntax ParseReceiveParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (parms, seps) = ParseSeparatedList(
            static @this => @this.ParseReceiveParameter(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private ReceiveParameterSyntax ParseReceiveParameter()
    {
        var pat = ParsePattern();

        return new(pat);
    }

    private TryExpressionSyntax ParseTryExpression()
    {
        var @try = Read();
        var body = ParseExpression();
        var @catch = Expect(SyntaxTokenKind.CatchKeyword);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (arms, seps) = ParseSeparatedList(
            static @this => @this.ParseExpressionPatternArm(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: false,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(@try, body, @catch, open, List(arms, seps), close);
    }

    private WhileExpressionSyntax ParseWhileExpression()
    {
        var @while = Read();
        var condition = ParseExpression();
        var body = ParseBlockExpression();
        var @else = ParseOptional(SyntaxTokenKind.ElseKeyword, static @this => @this.ParseExpressionElse());

        return new(@while, condition, body, @else);
    }

    private ForExpressionSyntax ParseForExpression()
    {
        var @for = Read();
        var pat = ParsePattern();
        var @in = Expect(SyntaxTokenKind.InKeyword);
        var collection = ParseExpression();
        var body = ParseBlockExpression();
        var @else = ParseOptional(SyntaxTokenKind.ElseKeyword, static @this => @this.ParseExpressionElse());

        return new(@for, pat, @in, collection, body, @else);
    }

    private ReturnExpressionSyntax ParseReturnExpression()
    {
        var tail = Optional(SyntaxTokenKind.TailKeyword);
        var ret = tail != null ? Expect(SyntaxTokenKind.RetKeyword) : Read();
        var oper = ParseExpression();

        return new(tail, ret, oper);
    }

    private RaiseExpressionSyntax ParseRaiseExpression()
    {
        var raise = Read();
        var oper = ParseExpression();

        return new(raise, oper);
    }

    private NextExpressionSyntax ParseNextExpression()
    {
        var next = Read();

        return new(next);
    }

    private BreakExpressionSyntax ParseBreakExpression()
    {
        var @break = Read();
        var result = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParseBreakExpressionResult());

        return new(@break, result);
    }

    private BreakExpressionResultSyntax ParseBreakExpressionResult()
    {
        var @as = Read();
        var value = ParseExpression();

        return new(@as, value);
    }

    private ExpressionSyntax ParsePostfixExpression(ExpressionSyntax subject)
    {
        while (Peek1() is { IsEndOfInput: false } next)
        {
            switch (next.Kind)
            {
                case SyntaxTokenKind.Dot:
                    subject = ParseFieldExpression(subject);
                    continue;
                case SyntaxTokenKind.OpenBracket:
                    subject = ParseIndexExpression(subject);
                    continue;
                case SyntaxTokenKind.OpenParen:
                    subject = ParseCallExpression(subject);
                    continue;
                case SyntaxTokenKind.MinusCloseAngle:
                    subject = ParseSendExpression(subject);
                    continue;
                default:
                    break;
            }

            break;
        }

        return subject;
    }

    private FieldExpressionSyntax ParseFieldExpression(ExpressionSyntax subject)
    {
        var dot = Read();
        var name = ExpectCodeIdentifier();

        return new(subject, dot, name);
    }

    private IndexExpressionSyntax ParseIndexExpression(ExpressionSyntax subject)
    {
        var args = ParseIndexArgumentList();

        return new(subject, args);
    }

    private IndexArgumentListSyntax ParseIndexArgumentList()
    {
        var open = Read();
        var (args, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: false,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(open, List(args, seps), close);
    }

    private CallExpressionSyntax ParseCallExpression(ExpressionSyntax subject)
    {
        var args = ParseCallArgumentList();
        var question = Optional(SyntaxTokenKind.Question);

        return new(subject, args, question);
    }

    private CallArgumentListSyntax ParseCallArgumentList()
    {
        var open = Read();
        var (args, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(args, seps), close);
    }

    private SendExpressionSyntax ParseSendExpression(ExpressionSyntax subject)
    {
        var arrow = Read();
        var name = ExpectCodeIdentifier();
        var args = ParseSendArgumentList();

        return new(subject, arrow, name, args);
    }

    private SendArgumentListSyntax ParseSendArgumentList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var (args, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseParen,
            allowEmpty: true,
            allowTrailing: false);
        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(args, seps), close);
    }

    // Bindings

    private BindingSyntax ParseBinding()
    {
        var next = Peek1();
        var binding = next?.Kind switch
        {
            { } kind when kind == SyntaxTokenKind.MutKeyword || SyntaxFacts.IsCodeIdentifier(kind) =>
                ParseVariableBinding(),
            SyntaxTokenKind.DiscardIdentifier => ParseDiscardBinding(),
            _ => default(BindingSyntax),
        };

        if (binding == null)
        {
            ErrorExpected(StandardDiagnosticCodes.MissingBinding, GetLocation(next), "binding");

            binding = new VariableBindingSyntax(null, Missing());
        }

        return binding;
    }

    private VariableBindingSyntax ParseVariableBinding()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var name = mut != null ? ExpectCodeIdentifier() : Read();

        return new(mut, name);
    }

    private DiscardBindingSyntax ParseDiscardBinding()
    {
        var name = Read();

        return new(name);
    }

    // Patterns

    private PatternSyntax ParsePattern()
    {
        var (tok1, tok2) = Peek2();
        var pat = (tok1?.Kind, tok2?.Kind) switch
        {
            (SyntaxTokenKind.MutKeyword, _) => ParseWildcardOrStringOrArrayPattern(),
            ({ } ident, _) when SyntaxFacts.IsBindingIdentifier(ident) => ParseWildcardOrStringOrArrayPattern(),
            _ when IsMinus(tok1) => ParseLiteralPattern(),
            (SyntaxTokenKind.StringLiteral, _) => ParseStringPattern(null),
            ({ } literal, _) when SyntaxFacts.IsLiteral(literal) => ParseLiteralPattern(),
            (SyntaxTokenKind.RecKeyword, _) => ParseRecordPattern(),
            (SyntaxTokenKind.ErrKeyword, _) => ParseErrorPattern(),
            (SyntaxTokenKind.OpenParen, _) => ParseTuplePattern(),
            (SyntaxTokenKind.OpenBracket, _) => ParseArrayPattern(null),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace) => ParseSetPattern(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket) => ParseMapPattern(),
            (SyntaxTokenKind.UpperIdentifier, _) => ParseModulePattern(),
            _ => null,
        };

        if (pat == null)
        {
            ErrorExpected(StandardDiagnosticCodes.MissingPattern, GetLocation(Peek1()), "pattern");

            pat = new WildcardPatternSyntax(new VariableBindingSyntax(null, Missing()), null);
        }

        return pat;
    }

    private PatternAliasSyntax ParsePatternAlias()
    {
        var @as = Read();
        var binding = ParseVariableBinding();

        return new(@as, binding);
    }

    private PatternSyntax ParseWildcardOrStringOrArrayPattern()
    {
        var binding = ParseBinding();

        switch (Peek2())
        {
            case ({ Kind: SyntaxTokenKind.ColonColon }, { Kind: SyntaxTokenKind.StringLiteral }):
                return ParseStringPattern(binding);
            case ({ Kind: SyntaxTokenKind.ColonColon }, { Kind: SyntaxTokenKind.OpenBracket }):
                return ParseArrayPattern(binding);
        }

        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new WildcardPatternSyntax(binding, alias);
    }

    private LiteralPatternSyntax ParseLiteralPattern()
    {
        var minus = OptionalMinus();
        var literal = minus != null ? ExpectNumericLiteral() : Read();
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(minus, literal, alias);
    }

    private StringPatternSyntax ParseStringPattern(BindingSyntax? binding)
    {
        var leftLit = default(SyntaxToken);
        var leftSep = default(SyntaxToken);
        var rightSep = default(SyntaxToken);
        var rightLit = default(SyntaxToken);

        if (binding != null)
        {
            rightSep = Read();
            rightLit = Read();
        }
        else
        {
            leftLit = Read();

            if (Optional(SyntaxTokenKind.ColonColon) is { } left)
            {
                leftSep = left;
                binding = ParseBinding();

                if (Optional(SyntaxTokenKind.ColonColon) is { } right)
                {
                    rightSep = right;
                    rightLit = Expect(SyntaxTokenKind.StringLiteral);
                }
            }
        }

        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(leftLit, leftSep, binding, rightSep, rightLit, alias);
    }

    private ModulePatternSyntax ParseModulePattern()
    {
        var path = ParseModulePath();
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(path, alias);
    }

    private RecordPatternSyntax ParseRecordPattern()
    {
        var rec = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregatePatternField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(rec, open, List(fields, seps), close, alias);
    }

    private ErrorPatternSyntax ParseErrorPattern()
    {
        var err = Read();
        var name = Optional(SyntaxTokenKind.UpperIdentifier);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (fields, seps) = ParseSeparatedList(
            static @this => @this.ParseAggregatePatternField(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(err, name, open, List(fields, seps), close, alias);
    }

    private AggregatePatternFieldSyntax ParseAggregatePatternField()
    {
        var name = ExpectCodeIdentifier();
        var colon = Expect(SyntaxTokenKind.Colon);
        var pat = ParsePattern();

        return new(name, colon, pat);
    }

    private TuplePatternSyntax ParseTuplePattern()
    {
        var open = Read();
        var (comps, seps) = SeparatedBuilder<PatternSyntax>();

        // TODO: Can we merge this with ParseSeparatedList?
        comps.Add(ParsePattern());
        seps.Add(Expect(SyntaxTokenKind.Comma));
        comps.Add(ParsePattern());

        while (Optional(SyntaxTokenKind.Comma) is { } sep)
        {
            seps.Add(sep);
            comps.Add(ParsePattern());
        }

        var close = Expect(SyntaxTokenKind.CloseParen);
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(open, List(comps, seps), close, alias);
    }

    private ArrayPatternSyntax ParseArrayPattern(BindingSyntax? binding)
    {
        var leftClause = default(ArrayPatternClauseSyntax);
        var leftSep = default(SyntaxToken);
        var rightSep = default(SyntaxToken);
        var rightClause = default(ArrayPatternClauseSyntax);

        if (binding != null)
        {
            rightSep = Read();
            rightClause = ParseArrayPatternClause();
        }
        else
        {
            leftClause = ParseArrayPatternClause();

            if (Optional(SyntaxTokenKind.ColonColon) is { } left)
            {
                leftSep = left;
                binding = ParseBinding();

                if (Optional(SyntaxTokenKind.ColonColon) is { } right)
                {
                    rightSep = right;
                    rightClause = ParseArrayPatternClause();
                }
            }
        }

        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(leftClause, leftSep, binding, rightSep, rightClause, alias);
    }

    private ArrayPatternClauseSyntax ParseArrayPatternClause()
    {
        var open = Expect(SyntaxTokenKind.OpenBracket);
        var (elems, seps) = ParseSeparatedList(
            static @this => @this.ParsePattern(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(open, List(elems, seps), close);
    }

    private SetPatternSyntax ParseSetPattern()
    {
        var hash = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var (elems, seps) = ParseSeparatedList(
            static @this => @this.ParseExpression(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBrace,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(hash, open, List(elems, seps), close, alias);
    }

    private MapPatternSyntax ParseMapPattern()
    {
        var hash = Read();
        var open = Expect(SyntaxTokenKind.OpenBracket);
        var (pairs, seps) = ParseSeparatedList(
            static @this => @this.ParseMapPatternPair(),
            SyntaxTokenKind.Comma,
            SyntaxTokenKind.CloseBracket,
            allowEmpty: true,
            allowTrailing: true);
        var close = Expect(SyntaxTokenKind.CloseBracket);
        var alias = ParseOptional(SyntaxTokenKind.AsKeyword, static @this => @this.ParsePatternAlias());

        return new(hash, open, List(pairs, seps), close, alias);
    }

    private MapPatternPairSyntax ParseMapPatternPair()
    {
        var key = ParseExpression();
        var colon = Expect(SyntaxTokenKind.Colon);
        var value = ParsePattern();

        return new(key, colon, value);
    }
}