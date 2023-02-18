using Vezel.Celerity.Syntax.Text;
using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax;

internal sealed class LanguageParser
{
    private readonly string _path;

    private readonly SyntaxInputReader<SyntaxToken> _reader;

    private readonly SyntaxMode _mode;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    private readonly SourceLocation _eoiLocation;

    public LanguageParser(
        string path,
        IReadOnlyList<SyntaxToken> tokens,
        SyntaxMode mode,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _path = path;
        _reader = new(tokens);
        _mode = mode;
        _diagnostics = diagnostics;
        _eoiLocation = tokens[tokens.Count - 1].Location;
    }

    private SyntaxToken? Peek1()
    {
        return _reader.Peek1() is (true, var tok) ? tok : null;
    }

    private (SyntaxToken First, SyntaxToken Second)? Peek2()
    {
        return _reader.Peek2() is (true, var tok1, var tok2) ? (tok1, tok2)! : null;
    }

    private (SyntaxToken First, SyntaxToken Second, SyntaxToken Third)? Peek3()
    {
        return _reader.Peek3() is (true, var tok1, var tok2, var tok3) ? (tok1, tok2, tok3)! : null;
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

        var missing = Missing();

        ErrorExpected(missing, next?.Location, "lowercase identifier");

        return missing;
    }

    private SyntaxToken ExpectBindingIdentifier()
    {
        var next = Peek1();

        // Same idea as above.
        if (next?.Kind is { } kind && SyntaxFacts.IsBindingIdentifier(kind))
            return Read();

        var missing = Missing();

        ErrorExpected(missing, next?.Location, "lowercase or discard identifier");

        return missing;
    }

    private SyntaxToken ExpectNumericLiteral()
    {
        var next = Peek1();

        if (next?.Kind is SyntaxTokenKind.IntegerLiteral or SyntaxTokenKind.RealLiteral)
            return Read();

        var missing = Missing();

        ErrorExpected(missing, next?.Location, "real or integer literal");

        return missing;
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

        var missing = Missing();

        ErrorExpected(missing, next?.Location, "literal");

        return missing;
    }

    private SyntaxToken Expect(SyntaxTokenKind kind1, SyntaxTokenKind kind2)
    {
        var next = Peek1();
        var kind = next?.Kind;

        if (kind == kind1 || kind == kind2)
            return Read();

        var missing = Missing();

        ErrorExpected(
            missing,
            next?.Location,
            $"{SyntaxFacts.GetFriendlyName(kind1)} or {SyntaxFacts.GetFriendlyName(kind2)}");

        return missing;
    }

    private SyntaxToken Expect(SyntaxTokenKind kind)
    {
        var next = Peek1();

        if (next?.Kind == kind)
            return Read();

        var missing = Missing();

        ErrorExpected(missing, next?.Location, SyntaxFacts.GetFriendlyName(kind));

        return missing;
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
        return Peek1()?.Kind == kind ? Read() : null;
    }

    private SyntaxToken Missing()
    {
        return new(_path);
    }

    private void ErrorExpected(SyntaxItem item, SourceLocation? location, string expected)
    {
        _diagnostics.Add(
            SourceDiagnostic.Create(
                item, SourceDiagnosticSeverity.Error, location ?? _eoiLocation, $"Expected {expected}"));
    }

    private static ImmutableArray<T>.Builder Builder<T>()
        where T : SyntaxItem
    {
        return ImmutableArray.CreateBuilder<T>();
    }

    private static SyntaxItemList<T> List<T>(ImmutableArray<T>.Builder elements)
        where T : SyntaxItem
    {
        return new(elements.ToImmutable());
    }

    private static SyntaxItemList<T> DrainList<T>(ImmutableArray<T>.Builder elements)
        where T : SyntaxItem
    {
        var list = new SyntaxItemList<T>(elements.ToImmutable());

        elements.Clear();

        return list;
    }

    private static SeparatedSyntaxItemList<TElement, SyntaxToken> List<TElement>(
        ImmutableArray<TElement>.Builder elements, ImmutableArray<SyntaxToken>.Builder separators)
        where TElement : SyntaxItem
    {
        return new(List(elements), List(separators));
    }

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

            void DrainToMissingDeclaration()
            {
                if (skipped.Count != 0)
                {
                    var first = skipped[0];

                    ErrorExpected(first, first.Location, "declaration");

                    decls.Add(new MissingDeclarationSyntax(DrainList(dattrs), Missing(), DrainList(skipped)));
                }
            }

            while (Peek1() is { IsEndOfInput: false } next)
            {
                if (SyntaxFacts.IsDeclarationStarter(next.Kind))
                {
                    DrainToMissingDeclaration();

                    decls.Add(ParseDeclaration(dattrs, true));

                    break;
                }

                skipped.Add(Read());
            }

            // There can be some leftover skipped tokens.
            DrainToMissingDeclaration();
        }

        var eoi = Expect(SyntaxTokenKind.EndOfInput);

        return new(List(mattrs), mod, path, semi, List(decls), eoi);
    }

    private InteractiveDocumentSyntax ParseInteractiveDocument()
    {
        var decls = Builder<DeclarationSyntax>();
        var stmts = Builder<StatementSyntax>();

        while (Peek1() is { IsEndOfInput: false })
        {
            var attrs = ParseAttributes();
            var skipped = Builder<SyntaxToken>();

            void DrainToMissingStatement(SyntaxToken? semicolon)
            {
                if (skipped.Count != 0 || semicolon != null)
                {
                    var first = skipped[0];

                    ErrorExpected(first, first.Location, "declaration or statement");

                    stmts.Add(new MissingStatementSyntax(DrainList(attrs), DrainList(skipped), semicolon ?? Missing()));
                }
            }

            while (Peek1() is { IsEndOfInput: false } next)
            {
                if (SyntaxFacts.IsDeclarationStarter(next.Kind))
                {
                    DrainToMissingStatement(null);

                    decls.Add(ParseDeclaration(attrs, true));

                    break;
                }

                if (SyntaxFacts.IsStatementStarter(next.Kind))
                {
                    DrainToMissingStatement(null);

                    stmts.Add(ParseStatement(attrs, true));

                    break;
                }

                if (next.Kind == SyntaxTokenKind.Semicolon)
                {
                    DrainToMissingStatement(Read());

                    break;
                }

                skipped.Add(Read());
            }

            // There can be some leftover skipped tokens.
            DrainToMissingStatement(null);
        }

        var eoi = Expect(SyntaxTokenKind.EndOfInput);

        return new(List(decls), List(stmts), eoi);
    }

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
        var value = Peek1()?.Kind == SyntaxTokenKind.Equals ? ParseAttributeValue() : null;

        return new(at, name, value);
    }

    private AttributeValueSyntax ParseAttributeValue()
    {
        var equals = Read();
        var value = ExpectLiteral();

        return new(equals, value);
    }

    private ModulePathSyntax ParseModulePath()
    {
        var idents = Builder<SyntaxToken>();
        var seps = Builder<SyntaxToken>();

        idents.Add(Expect(SyntaxTokenKind.UpperIdentifier));

        while (Peek1()?.Kind == SyntaxTokenKind.ColonColon)
        {
            seps.Add(Read());
            idents.Add(Expect(SyntaxTokenKind.UpperIdentifier));
        }

        return new(List(idents, seps));
    }

    private DeclarationSyntax ParseDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes, bool interactive)
    {
        var next2 = Peek2()!;

        return (next2?.First.Kind, next2?.Second.Kind) switch
        {
            (SyntaxTokenKind.UseKeyword, _) => ParseUseDeclaration(attributes),
            (SyntaxTokenKind.TypeKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.OpaqueKeyword or SyntaxTokenKind.TypeKeyword)
                when !interactive => ParseTypeDeclaration(attributes),
            (SyntaxTokenKind.ConstKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.ConstKeyword) => ParseConstantDeclaration(attributes),
            (SyntaxTokenKind.FnKeyword, _) or
            (SyntaxTokenKind.PubKeyword, SyntaxTokenKind.ExtKeyword or SyntaxTokenKind.FnKeyword) =>
                ParseFunctionDeclaration(attributes),
            (SyntaxTokenKind.TestKeyword, _) when !interactive => ParseTestDeclaration(attributes),
            _ => throw new UnreachableException(),
        };
    }

    private UseDeclarationSyntax ParseUseDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var use = Read();
        var name = Expect(SyntaxTokenKind.UpperIdentifier);
        var equals = Expect(SyntaxTokenKind.Equals);
        var path = ParseModulePath();

        return new(List(attributes), use, name, equals, path);
    }

    private TypeDeclarationSyntax ParseTypeDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var pub = Optional(SyntaxTokenKind.PubKeyword);
        var opaque = pub != null ? Optional(SyntaxTokenKind.OpaqueKeyword) : null;
        var kw = Expect(SyntaxTokenKind.TypeKeyword);
        var name = Expect(SyntaxTokenKind.LowerIdentifier);
        var parms = Peek1()?.Kind == SyntaxTokenKind.OpenParen ? ParseTypeParameterList() : null;
        var equals = Expect(SyntaxTokenKind.Equals);
        var type = ParseType();

        return new(List(attributes), pub, opaque, kw, name, parms, equals, type);
    }

    private TypeParameterListSyntax ParseTypeParameterList()
    {
        var open = Read();
        var parms = Builder<TypeParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        parms.Add(ParseTypeParameter());

        while (Peek1()?.Kind == SyntaxTokenKind.Comma)
        {
            seps.Add(Read());
            parms.Add(ParseTypeParameter());
        }

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
        var type = Peek1()?.Kind == SyntaxTokenKind.Colon ? ParseTypeAnnotation() : null;
        var equals = Expect(SyntaxTokenKind.Equals);
        var body = ParseExpression();

        return new(List(attributes), pub, @const, name, type, equals, body);
    }

    private FunctionDeclarationSyntax ParseFunctionDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var pub = Optional(SyntaxTokenKind.PubKeyword);
        var ext = Optional(SyntaxTokenKind.ExtKeyword);
        var fn = Expect(SyntaxTokenKind.FnKeyword);
        var name = ExpectCodeIdentifier();
        var parms = ParseFunctionParameterList();
        var err = Optional(SyntaxTokenKind.ErrKeyword);
        var type = Peek1()?.Kind == SyntaxTokenKind.MinusCloseAngle ? ParseReturnTypeAnnotation() : null;
        var body = ext == null ? ParseBlockExpression() : null;

        return new(List(attributes), pub, ext, fn, name, parms, err, type, body);
    }

    private FunctionParameterListSyntax ParseFunctionParameterList()
    {
        var open = Read();
        var parms = Builder<FunctionParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            parms.Add(ParseFunctionParameter());

            // TODO: The way we parse the parameter list (and other similar syntax nodes) causes the parser to
            // misinterpret the entire function body for some invalid inputs. We need to do better here.
            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                parms.Add(ParseFunctionParameter());
            }
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private FunctionParameterSyntax ParseFunctionParameter()
    {
        var attrs = ParseAttributes();
        var name = ExpectBindingIdentifier();
        var type = Peek1()?.Kind == SyntaxTokenKind.Colon ? ParseTypeAnnotation() : null;

        return new(List(attrs), name, type);
    }

    private TestDeclarationSyntax ParseTestDeclaration(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var test = Read();
        var name = ExpectCodeIdentifier();
        var body = ParseBlockExpression();

        return new(List(attributes), test, name, body);
    }

    private TypeSyntax ParseType()
    {
        var next3 = Peek3();
        var type = (next3?.First.Kind, next3?.Second.Kind, next3?.Third.Kind) switch
        {
            (SyntaxTokenKind.AnyKeyword, _, _) => ParseAnyType(),
            _ when IsMinus(next3?.First) => ParseLiteralType(),
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
            _ => Unsafe.As<TypeSyntax>(new MissingTypeSyntax()),
        };

        if (type is MissingTypeSyntax)
            ErrorExpected(type, Peek1()?.Location, "type");

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
        var range = Peek1()?.Kind == SyntaxTokenKind.OpenParen ? ParseIntegerTypeRange() : null;

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
        var fields = Builder<AggregateTypeFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregateTypeField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(rec, open, List(fields, seps), close);
    }

    private ErrorTypeSyntax ParseErrorType()
    {
        var err = Read();
        var name = Optional(SyntaxTokenKind.UpperIdentifier);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var fields = Builder<AggregateTypeFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregateTypeField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

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
        var comps = Builder<TypeSyntax>();
        var seps = Builder<SyntaxToken>();

        comps.Add(ParseType());
        seps.Add(Expect(SyntaxTokenKind.Comma));
        comps.Add(ParseType());

        while (Peek1()?.Kind == SyntaxTokenKind.Comma)
        {
            seps.Add(Read());
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
        var pairs = Builder<MapTypePairSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBracket })
        {
            pairs.Add(ParseMapTypePair());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

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
        var parms = ParseFunctionTypeParameterList();
        var err = Optional(SyntaxTokenKind.ErrKeyword);
        var type = ParseReturnTypeAnnotation();

        return new(fn, parms, err, type);
    }

    private FunctionTypeParameterListSyntax ParseFunctionTypeParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var parms = Builder<FunctionTypeParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            parms.Add(ParseFunctionTypeParameter());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                parms.Add(ParseFunctionTypeParameter());
            }
        }

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
        var msgs = Builder<AgentTypeMessageSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            msgs.Add(ParseAgentTypeMessage());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

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
        var parms = Builder<AgentTypeMessageParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            parms.Add(ParseAgentTypeMessageParameter());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                parms.Add(ParseAgentTypeMessageParameter());
            }
        }

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
        var path = Peek1()?.Kind == SyntaxTokenKind.UpperIdentifier ? ParseNominalTypePath() : null;
        var name = Expect(SyntaxTokenKind.LowerIdentifier);
        var args = Peek1()?.Kind == SyntaxTokenKind.OpenParen ? ParseNominalTypeArgumentList() : null;

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
        var args = Builder<TypeSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            args.Add(ParseType());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                args.Add(ParseType());
            }
        }

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

    private StatementSyntax ParseStatement(ImmutableArray<AttributeSyntax>.Builder attributes, bool interactive)
    {
        return Peek1()?.Kind switch
        {
            SyntaxTokenKind.LetKeyword => ParseLetStatement(attributes),
            SyntaxTokenKind.UseKeyword when !interactive => ParseUseStatement(attributes),
            SyntaxTokenKind.DeferKeyword when !interactive => ParseDeferStatement(attributes),
            SyntaxTokenKind.AssertKeyword when !interactive => ParseAssertStatement(attributes),
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

    private UseStatementSyntax ParseUseStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var use = Read();
        var pat = ParsePattern();
        var equals = Expect(SyntaxTokenKind.Equals);
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), use, pat, equals, expr, semi);
    }

    private ExpressionStatementSyntax ParseExpressionStatement(ImmutableArray<AttributeSyntax>.Builder attributes)
    {
        var expr = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(List(attributes), expr, semi);
    }

    private ExpressionSyntax ParseExpression()
    {
        return ParseAssignmentExpression();
    }

    private ExpressionSyntax ParseAssignmentExpression()
    {
        var expr = ParseLogicalExpression();

        while (Peek1()?.Kind == SyntaxTokenKind.Equals)
        {
            var op = Read();
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

        while (Peek1()?.Kind == SyntaxTokenKind.BitwiseOperator)
        {
            var op = Read();
            var right = ParseShiftExpression();

            expr = new BitwiseExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseShiftExpression()
    {
        var expr = ParseAdditiveExpression();

        while (Peek1()?.Kind == SyntaxTokenKind.ShiftOperator)
        {
            var op = Read();
            var right = ParseAdditiveExpression();

            expr = new ShiftExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseAdditiveExpression()
    {
        var expr = ParseMultiplicativeExpression();

        while (Peek1()?.Kind == SyntaxTokenKind.AdditiveOperator)
        {
            var op = Read();
            var right = ParseMultiplicativeExpression();

            expr = new AdditiveExpressionSyntax(expr, op, right);
        }

        return expr;
    }

    private ExpressionSyntax ParseMultiplicativeExpression()
    {
        var expr = ParsePrefixExpression();

        while (Peek1()?.Kind == SyntaxTokenKind.MultiplicativeOperator)
        {
            var op = Read();
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
        var next3 = Peek3();
        var expr = (next3?.First.Kind, next3?.Second.Kind, next3?.Third.Kind) switch
        {
            (SyntaxTokenKind.OpenParen, _, _) => ParseParenthesizedOrTupleExpression(),
            (SyntaxTokenKind.OpenBrace, _, _) => ParseBlockExpression(),
            ({ } ident, _, _) when SyntaxFacts.IsBindingIdentifier(ident) => ParseIdentifierExpression(),
            ({ } literal, _, _) when SyntaxFacts.IsLiteral(literal) => ParseLiteralExpression(),
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
            (SyntaxTokenKind.WhileKeyword, _, _) => ParseWhileExpression(),
            (SyntaxTokenKind.ForKeyword, _, _) => ParseForExpression(),
            (SyntaxTokenKind.RetKeyword or SyntaxTokenKind.TailKeyword, _, _) => ParseReturnExpression(),
            (SyntaxTokenKind.RaiseKeyword, _, _) => ParseRaiseExpression(),
            (SyntaxTokenKind.NextKeyword, _, _) => ParseNextExpression(),
            (SyntaxTokenKind.BreakKeyword, _, _) => ParseBreakExpression(),
            _ => new MissingExpressionSyntax(),
        };

        if (expr is MissingExpressionSyntax)
            ErrorExpected(expr, Peek1()?.Location, "expression");

        return ParsePostfixExpression(expr);
    }

    private ExpressionSyntax ParseParenthesizedOrTupleExpression()
    {
        var open = Read();
        var expr = ParseExpression();

        if (Peek1()?.Kind == SyntaxTokenKind.Comma)
        {
            var exprs = Builder<ExpressionSyntax>();
            var seps = Builder<SyntaxToken>();

            exprs.Add(expr);
            seps.Add(Read());
            exprs.Add(ParseExpression());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
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

        while (Peek1()?.Kind != SyntaxTokenKind.CloseBrace)
        {
            var attrs = ParseAttributes();
            var skipped = Builder<SyntaxToken>();

            void DrainToMissingStatement(SyntaxToken? semicolon)
            {
                if (skipped.Count != 0 || semicolon != null)
                {
                    var first = skipped[0];

                    ErrorExpected(first, first.Location, "statement");

                    stmts.Add(new MissingStatementSyntax(DrainList(attrs), DrainList(skipped), semicolon ?? Missing()));
                }
            }

            var decl = false;

            while (Peek1() is { IsEndOfInput: false } next)
            {
                if (SyntaxFacts.IsStatementStarter(next.Kind))
                {
                    DrainToMissingStatement(null);

                    stmts.Add(ParseStatement(attrs, false));

                    break;
                }

                if (SyntaxFacts.IsDeclarationStarter(next.Kind))
                {
                    DrainToMissingStatement(null);

                    // We are probably looking at a declaration because the user has not yet closed the current block
                    // expression. Stop parsing this block so we can properly parse the declaration.
                    decl = true;

                    break;
                }

                if (next.Kind == SyntaxTokenKind.Semicolon)
                {
                    DrainToMissingStatement(Read());

                    break;
                }

                skipped.Add(Read());
            }

            // There can be some leftover skipped tokens.
            DrainToMissingStatement(null);

            if (decl)
                break;
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(open, List(stmts), close);
    }

    private IdentifierExpressionSyntax ParseIdentifierExpression()
    {
        var name = Read();

        return new(name);
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
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var fields = Builder<AggregateExpressionFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregateExpressionField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(rec, open, List(fields, seps), close);
    }

    private ErrorExpressionSyntax ParseErrorExpression()
    {
        var err = Read();
        var name = Expect(SyntaxTokenKind.UpperIdentifier);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var fields = Builder<AggregateExpressionFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregateExpressionField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(err, name, open, List(fields, seps), close);
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
        var elems = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBracket })
        {
            elems.Add(ParseExpression());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(mut, open, List(elems, seps), close);
    }

    private SetExpressionSyntax ParseSetExpression()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var elems = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            elems.Add(ParseExpression());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(mut, hash, open, List(elems, seps), close);
    }

    private MapExpressionSyntax ParseMapExpression()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var hash = Read();
        var open = Read();
        var pairs = Builder<MapExpressionPairSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBracket })
        {
            pairs.Add(ParseMapExpressionPair());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

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
        var arrow = Expect(SyntaxTokenKind.EqualsCloseAngle);
        var body = ParseExpression();

        return new(fn, parms, arrow, body);
    }

    private LambdaParameterListSyntax ParseLambdaParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var parms = Builder<LambdaParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            parms.Add(ParseLambdaParameter());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                parms.Add(ParseLambdaParameter());
            }
        }

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
        var @else = Peek1()?.Kind == SyntaxTokenKind.ElseKeyword ? ParseExpressionElse() : null;

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
        var arms = Builder<ConditionExpressionArmSyntax>();

        arms.Add(ParseConditionExpressionArm());

        while (Peek1()?.Kind != SyntaxTokenKind.CloseBrace)
            arms.Add(ParseConditionExpressionArm());

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(cond, open, List(arms), close);
    }

    private ConditionExpressionArmSyntax ParseConditionExpressionArm()
    {
        var condition = ParseExpression();
        var arrow = Expect(SyntaxTokenKind.EqualsCloseAngle);
        var body = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(condition, arrow, body, semi);
    }

    private MatchExpressionSyntax ParseMatchExpression()
    {
        var match = Read();
        var oper = ParseExpression();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var arms = Builder<ExpressionPatternArmSyntax>();

        arms.Add(ParseExpressionPatternArm());

        while (Peek1()?.Kind != SyntaxTokenKind.CloseBrace)
            arms.Add(ParseExpressionPatternArm());

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(match, oper, open, List(arms), close);
    }

    private ExpressionPatternArmSyntax ParseExpressionPatternArm()
    {
        var pat = ParsePattern();
        var guard = Peek1()?.Kind == SyntaxTokenKind.IfKeyword ? ParseExpressionArmGuard() : null;
        var arrow = Expect(SyntaxTokenKind.EqualsCloseAngle);
        var body = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(pat, guard, arrow, body, semi);
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
        var arms = Builder<ReceiveExpressionArmSyntax>();

        arms.Add(ParseReceiveExpressionArm());

        while (Peek1()?.Kind != SyntaxTokenKind.CloseBrace)
            arms.Add(ParseReceiveExpressionArm());

        var close = Expect(SyntaxTokenKind.CloseBrace);
        var @else = Peek1()?.Kind == SyntaxTokenKind.ElseKeyword ? ParseExpressionElse() : null;

        return new(recv, open, List(arms), close, @else);
    }

    private ReceiveExpressionArmSyntax ParseReceiveExpressionArm()
    {
        var name = ExpectCodeIdentifier();
        var parms = ParseReceiveParameterList();
        var guard = Peek1()?.Kind == SyntaxTokenKind.IfKeyword ? ParseExpressionArmGuard() : null;
        var arrow = Expect(SyntaxTokenKind.EqualsCloseAngle);
        var body = ParseExpression();
        var semi = Expect(SyntaxTokenKind.Semicolon);

        return new(name, parms, guard, arrow, body, semi);
    }

    private ReceiveParameterListSyntax ParseReceiveParameterList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var parms = Builder<ReceiveParameterSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            parms.Add(ParseReceiveParameter());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                parms.Add(ParseReceiveParameter());
            }
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(parms, seps), close);
    }

    private ReceiveParameterSyntax ParseReceiveParameter()
    {
        var pat = ParsePattern();

        return new(pat);
    }

    private WhileExpressionSyntax ParseWhileExpression()
    {
        var @while = Read();
        var condition = ParseExpression();
        var body = ParseBlockExpression();
        var @else = Peek1()?.Kind == SyntaxTokenKind.ElseKeyword ? ParseExpressionElse() : null;

        return new(@while, condition, body, @else);
    }

    private ForExpressionSyntax ParseForExpression()
    {
        var @for = Read();
        var pat = ParsePattern();
        var @in = Expect(SyntaxTokenKind.InKeyword);
        var collection = ParseExpression();
        var body = ParseBlockExpression();
        var @else = Peek1()?.Kind == SyntaxTokenKind.ElseKeyword ? ParseExpressionElse() : null;

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
        var result = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParseBreakExpressionResult() : null;

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
        var open = Expect(SyntaxTokenKind.OpenBracket);
        var args = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseBracket)
        {
            args.Add(ParseExpression());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                args.Add(ParseExpression());
            }
        }

        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(open, List(args, seps), close);
    }

    private CallExpressionSyntax ParseCallExpression(ExpressionSyntax subject)
    {
        var args = ParseCallArgumentList();
        var @try = Peek1()?.Kind == SyntaxTokenKind.Question ? ParseCallExpressionTry() : null;

        return new(subject, args, @try);
    }

    private CallArgumentListSyntax ParseCallArgumentList()
    {
        var open = Expect(SyntaxTokenKind.OpenParen);
        var args = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            args.Add(ParseExpression());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                args.Add(ParseExpression());
            }
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(args, seps), close);
    }

    private CallExpressionTrySyntax ParseCallExpressionTry()
    {
        var question = Read();
        var @catch = Peek1()?.Kind == SyntaxTokenKind.CatchKeyword ? ParseCallExpressionTryCatch() : null;

        return new(question, @catch);
    }

    private CallExpressionTryCatchSyntax ParseCallExpressionTryCatch()
    {
        var @catch = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var arms = Builder<ExpressionPatternArmSyntax>();

        arms.Add(ParseExpressionPatternArm());

        while (Peek1()?.Kind != SyntaxTokenKind.CloseBrace)
            arms.Add(ParseExpressionPatternArm());

        var close = Expect(SyntaxTokenKind.CloseBrace);

        return new(@catch, open, List(arms), close);
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
        var args = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        if (Peek1()?.Kind != SyntaxTokenKind.CloseParen)
        {
            args.Add(ParseExpression());

            while (Peek1()?.Kind == SyntaxTokenKind.Comma)
            {
                seps.Add(Read());
                args.Add(ParseExpression());
            }
        }

        var close = Expect(SyntaxTokenKind.CloseParen);

        return new(open, List(args, seps), close);
    }

    private PatternSyntax ParsePattern()
    {
        var next2 = Peek2();
        var pat = (next2?.First.Kind, next2?.Second.Kind) switch
        {
            (SyntaxTokenKind.MutKeyword, _) => ParseWildcardOrStringOrArrayPattern(),
            ({ } ident, _) when SyntaxFacts.IsBindingIdentifier(ident) => ParseWildcardOrStringOrArrayPattern(),
            _ when IsMinus(next2?.First) => ParseLiteralPattern(),
            (SyntaxTokenKind.StringLiteral, _) => ParseStringPattern(null),
            ({ } literal, _) when SyntaxFacts.IsLiteral(literal) => ParseLiteralPattern(),
            (SyntaxTokenKind.RecKeyword, _) => ParseRecordPattern(),
            (SyntaxTokenKind.ErrKeyword, _) => ParseErrorPattern(),
            (SyntaxTokenKind.OpenParen, _) => ParseTuplePattern(),
            (SyntaxTokenKind.OpenBracket, _) => ParseArrayPattern(null),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBrace) => ParseSetPattern(),
            (SyntaxTokenKind.Hash, SyntaxTokenKind.OpenBracket) => ParseMapPattern(),
            (SyntaxTokenKind.UpperIdentifier, _) => ParseModulePattern(),
            _ => new MissingPatternSyntax(null),
        };

        if (pat is MissingPatternSyntax)
            ErrorExpected(pat, Peek1()?.Location, "pattern");

        return pat;
    }

    private PatternAliasSyntax ParsePatternAlias()
    {
        var @as = Read();
        var binding = ParseVariablePatternBinding();

        return new(@as, binding);
    }

    private PatternBindingSyntax ParsePatternBinding()
    {
        var next = Peek1();
        var binding = next?.Kind switch
        {
            { } kind when kind == SyntaxTokenKind.MutKeyword || SyntaxFacts.IsCodeIdentifier(kind) =>
                ParseVariablePatternBinding(),
            SyntaxTokenKind.DiscardIdentifier => ParseDiscardPatternBinding(),
            _ => Unsafe.As<PatternBindingSyntax>(new MissingPatternBindingSyntax(Missing())),
        };

        if (binding is MissingPatternBindingSyntax)
            ErrorExpected(binding, next?.Location, "pattern binding");

        return binding;
    }

    private VariablePatternBindingSyntax ParseVariablePatternBinding()
    {
        var mut = Optional(SyntaxTokenKind.MutKeyword);
        var name = mut != null ? ExpectCodeIdentifier() : Read();

        return new(mut, name);
    }

    private DiscardPatternBindingSyntax ParseDiscardPatternBinding()
    {
        var name = Read();

        return new(name);
    }

    private PatternSyntax ParseWildcardOrStringOrArrayPattern()
    {
        var binding = ParsePatternBinding();

        switch (Peek2())
        {
            case ({ Kind: SyntaxTokenKind.ColonColon }, { Kind: SyntaxTokenKind.StringLiteral }):
                return ParseStringPattern(binding);
            case ({ Kind: SyntaxTokenKind.ColonColon }, { Kind: SyntaxTokenKind.OpenBracket }):
                return ParseArrayPattern(binding);
        }

        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new WildcardPatternSyntax(binding, alias);
    }

    private LiteralPatternSyntax ParseLiteralPattern()
    {
        var minus = OptionalMinus();
        var literal = minus != null ? ExpectNumericLiteral() : Read();
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(minus, literal, alias);
    }

    private StringPatternSyntax ParseStringPattern(PatternBindingSyntax? binding)
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

            if (Peek1()?.Kind == SyntaxTokenKind.ColonColon)
            {
                leftSep = Read();
                binding = ParsePatternBinding();

                if (Peek1()?.Kind == SyntaxTokenKind.ColonColon)
                {
                    rightSep = Read();
                    rightLit = Expect(SyntaxTokenKind.StringLiteral);
                }
            }
        }

        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(leftLit, leftSep, binding, rightSep, rightLit, alias);
    }

    private ModulePatternSyntax ParseModulePattern()
    {
        var path = ParseModulePath();
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(path, alias);
    }

    private RecordPatternSyntax ParseRecordPattern()
    {
        var rec = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var fields = Builder<AggregatePatternFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregatePatternField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(rec, open, List(fields, seps), close, alias);
    }

    private ErrorPatternSyntax ParseErrorPattern()
    {
        var err = Read();
        var name = Optional(SyntaxTokenKind.UpperIdentifier);
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var fields = Builder<AggregatePatternFieldSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            fields.Add(ParseAggregatePatternField());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

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
        var comps = Builder<PatternSyntax>();
        var seps = Builder<SyntaxToken>();

        comps.Add(ParsePattern());
        seps.Add(Expect(SyntaxTokenKind.Comma));
        comps.Add(ParsePattern());

        while (Peek1()?.Kind == SyntaxTokenKind.Comma)
        {
            seps.Add(Read());
            comps.Add(ParsePattern());
        }

        var close = Expect(SyntaxTokenKind.CloseParen);
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(open, List(comps, seps), close, alias);
    }

    private ArrayPatternSyntax ParseArrayPattern(PatternBindingSyntax? binding)
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

            if (Peek1()?.Kind == SyntaxTokenKind.ColonColon)
            {
                leftSep = Read();
                binding = ParsePatternBinding();

                if (Peek1()?.Kind == SyntaxTokenKind.ColonColon)
                {
                    rightSep = Read();
                    rightClause = ParseArrayPatternClause();
                }
            }
        }

        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(leftClause, leftSep, binding, rightSep, rightClause, alias);
    }

    private ArrayPatternClauseSyntax ParseArrayPatternClause()
    {
        var open = Expect(SyntaxTokenKind.OpenBracket);
        var elems = Builder<PatternSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBracket })
        {
            elems.Add(ParsePattern());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBracket);

        return new(open, List(elems, seps), close);
    }

    private MapPatternSyntax ParseMapPattern()
    {
        var hash = Read();
        var open = Expect(SyntaxTokenKind.OpenBracket);
        var pairs = Builder<MapPatternPairSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBracket })
        {
            pairs.Add(ParseMapPatternPair());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBracket);
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(hash, open, List(pairs, seps), close, alias);
    }

    private MapPatternPairSyntax ParseMapPatternPair()
    {
        var key = ParseExpression();
        var colon = Expect(SyntaxTokenKind.Colon);
        var value = ParsePattern();

        return new(key, colon, value);
    }

    private SetPatternSyntax ParseSetPattern()
    {
        var hash = Read();
        var open = Expect(SyntaxTokenKind.OpenBrace);
        var elems = Builder<ExpressionSyntax>();
        var seps = Builder<SyntaxToken>();

        while (Peek1() is { IsEndOfInput: false, Kind: not SyntaxTokenKind.CloseBrace })
        {
            elems.Add(ParseExpression());

            if (Peek1()?.Kind != SyntaxTokenKind.Comma)
                break;

            seps.Add(Read());
        }

        var close = Expect(SyntaxTokenKind.CloseBrace);
        var alias = Peek1()?.Kind == SyntaxTokenKind.AsKeyword ? ParsePatternAlias() : null;

        return new(hash, open, List(elems, seps), close, alias);
    }
}
