namespace Vezel.Celerity.Language.Tooling.Classification;

public static class TextClassifier
{
    public static IEnumerable<ClassifiedSourceTextSpan> ClassifySyntactically(SyntaxNode node, SourceTextSpan span)
    {
        Check.Null(node);
        Check.Argument(node.FullSpan.Contains(span), span);

        var classifications = new List<ClassifiedSourceTextSpan>();

        ClassifyTerminals(node, span, classifications);
        ClassifySyntaxNodes(node, span, classifications, identifiers: true);

        classifications.Sort((x, y) => x.Span.CompareTo(y.Span));

        return classifications;
    }

    public static IEnumerable<ClassifiedSourceTextSpan> ClassifySemantically(SemanticNode node, SourceTextSpan span)
    {
        Check.Null(node);
        Check.Argument(node.Syntax.FullSpan.Contains(span), span);

        var classifications = new List<ClassifiedSourceTextSpan>();

        ClassifyTerminals(node.Syntax, span, classifications);
        ClassifySyntaxNodes(node.Syntax, span, classifications, identifiers: false);
        ClassifySemanticNodes(node, span, classifications);

        classifications.Sort((x, y) => x.Span.CompareTo(y.Span));

        return classifications;
    }

    private static void ClassifyTerminals(
        SyntaxNode node, SourceTextSpan span, List<ClassifiedSourceTextSpan> classifications)
    {
        foreach (var terminal in node.DescendantTerminals())
        {
            // Span is equivalent to FullSpan for trivia. For tokens, we only care about Span anyway.
            if (!span.Overlaps(terminal.Span))
                continue;

            // We handle the terminals that we can unambiguously classify without looking at nodes.
            var classification = terminal switch
            {
                SyntaxTrivia trivia => trivia.Kind switch
                {
                    SyntaxTriviaKind.ShebangLine => SyntaxClassification.ShebangLine,
                    SyntaxTriviaKind.WhiteSpace or
                    SyntaxTriviaKind.NewLine => SyntaxClassification.WhiteSpace,
                    SyntaxTriviaKind.Comment => SyntaxClassification.Comment,
                    _ => default(SyntaxClassification?),
                },
                SyntaxToken token => token.Kind switch
                {
                    SyntaxTokenKind.BitwiseOperator or
                    SyntaxTokenKind.ShiftOperator or
                    SyntaxTokenKind.MultiplicativeOperator or
                    SyntaxTokenKind.AdditiveOperator or
                    SyntaxTokenKind.Equals or
                    SyntaxTokenKind.EqualsEquals or
                    SyntaxTokenKind.ExclamationEquals or
                    SyntaxTokenKind.CloseAngle or
                    SyntaxTokenKind.CloseAngleEquals or
                    SyntaxTokenKind.OpenAngle or
                    SyntaxTokenKind.OpenAngleEquals => SyntaxClassification.Operator,
                    SyntaxTokenKind.Dot or
                    SyntaxTokenKind.DotDot or
                    SyntaxTokenKind.Comma or
                    SyntaxTokenKind.Colon or
                    SyntaxTokenKind.ColonColon or
                    SyntaxTokenKind.Semicolon or
                    SyntaxTokenKind.MinusCloseAngle or
                    SyntaxTokenKind.At or
                    SyntaxTokenKind.Hash or
                    SyntaxTokenKind.Question or
                    SyntaxTokenKind.OpenParen or
                    SyntaxTokenKind.CloseParen or
                    SyntaxTokenKind.OpenBracket or
                    SyntaxTokenKind.CloseBracket or
                    SyntaxTokenKind.OpenBrace or
                    SyntaxTokenKind.CloseBrace => SyntaxClassification.Punctuator,
                    SyntaxTokenKind.ConstKeyword or
                    SyntaxTokenKind.ExtKeyword or
                    SyntaxTokenKind.OpaqueKeyword or
                    SyntaxTokenKind.PubKeyword or
                    SyntaxTokenKind.TestKeyword or
                    SyntaxTokenKind.TypeKeyword or
                    SyntaxTokenKind.UseKeyword => SyntaxClassification.DeclarationKeyword,
                    SyntaxTokenKind.AsKeyword or
                    SyntaxTokenKind.AssertKeyword or
                    SyntaxTokenKind.DeferKeyword or
                    SyntaxTokenKind.LetKeyword or
                    SyntaxTokenKind.AndKeyword or
                    SyntaxTokenKind.BreakKeyword or
                    SyntaxTokenKind.CatchKeyword or
                    SyntaxTokenKind.CondKeyword or
                    SyntaxTokenKind.ElseKeyword or
                    SyntaxTokenKind.ForKeyword or
                    SyntaxTokenKind.IfKeyword or
                    SyntaxTokenKind.InKeyword or
                    SyntaxTokenKind.MatchKeyword or
                    SyntaxTokenKind.MetaKeyword or
                    SyntaxTokenKind.MutKeyword or
                    SyntaxTokenKind.NextKeyword or
                    SyntaxTokenKind.NotKeyword or
                    SyntaxTokenKind.OrKeyword or
                    SyntaxTokenKind.RaiseKeyword or
                    SyntaxTokenKind.RecKeyword or
                    SyntaxTokenKind.RecvKeyword or
                    SyntaxTokenKind.RetKeyword or
                    SyntaxTokenKind.TailKeyword or
                    SyntaxTokenKind.ThisKeyword or
                    SyntaxTokenKind.TryKeyword or
                    SyntaxTokenKind.WhileKeyword or
                    SyntaxTokenKind.WithKeyword => SyntaxClassification.OperationKeyword,
                    SyntaxTokenKind.FriendKeyword or
                    SyntaxTokenKind.MacroKeyword or
                    SyntaxTokenKind.QuoteKeyword or
                    SyntaxTokenKind.UnquoteKeyword or
                    SyntaxTokenKind.YieldKeyword => SyntaxClassification.ReservedKeyword,
                    SyntaxTokenKind.NilLiteral => SyntaxClassification.NilLiteral,
                    SyntaxTokenKind.BooleanLiteral => SyntaxClassification.BooleanLiteral,
                    SyntaxTokenKind.IntegerLiteral => SyntaxClassification.IntegerLiteral,
                    SyntaxTokenKind.RealLiteral => SyntaxClassification.RealLiteral,
                    SyntaxTokenKind.AtomLiteral => SyntaxClassification.AtomLiteral,
                    SyntaxTokenKind.StringLiteral => SyntaxClassification.StringLiteral,
                    _ => null,
                },
                _ => throw new UnreachableException(),
            };

            if (classification is { } cls)
                classifications.Add(new(terminal.Span, cls, SyntaxClassificationModifiers.None));
        }
    }

    private static void ClassifySyntaxNodes(
        SyntaxNode node, SourceTextSpan span, List<ClassifiedSourceTextSpan> classifications, bool identifiers)
    {
        void Classify(
            SyntaxToken? token,
            SyntaxClassification classification,
            bool @public = false,
            bool opaque = false,
            bool external = false,
            bool fallible = false,
            bool mutable = false,
            bool discard = false)
        {
            if (token is not { IsMissing: false })
                return;

            var modifiers = SyntaxClassificationModifiers.None;

            if (@public)
                modifiers |= SyntaxClassificationModifiers.Public;

            if (opaque)
                modifiers |= SyntaxClassificationModifiers.Opaque;

            if (external)
                modifiers |= SyntaxClassificationModifiers.External;

            if (fallible)
                modifiers |= SyntaxClassificationModifiers.Fallible;

            if (mutable)
                modifiers |= SyntaxClassificationModifiers.Mutable;

            if (discard)
                modifiers |= SyntaxClassificationModifiers.Discard;

            classifications.Add(new(token.Span, classification, modifiers));
        }

        foreach (var child in node.DescendantNodesAndSelf())
        {
            // We are only interested in child tokens here, so no need to check FullSpan.
            if (!span.Overlaps(child.Span))
                continue;

            switch (child)
            {
                case ModuleDocumentSyntax modDoc:
                    Classify(modDoc.ModKeywordToken, SyntaxClassification.DeclarationKeyword);

                    break;
                case AttributeSyntax attr:
                    Classify(attr.NameToken, SyntaxClassification.AttributeName);

                    break;
                case ModulePathSyntax modulePath:
                    foreach (var token in modulePath.ComponentTokens)
                        Classify(Unsafe.As<SyntaxToken>(token), SyntaxClassification.ModuleName);

                    break;
                case CodeParameterSyntax { NameToken: var name }:
                    Classify(name, SyntaxClassification.CodeParameterName, discard: name.Text.StartsWith('_'));

                    break;
                case TypeDeclarationSyntax typeDecl:
                    Classify(
                        typeDecl.NameToken,
                        SyntaxClassification.TypeParameterName,
                        @public: typeDecl.PubKeywordToken != null,
                        opaque: typeDecl.OpaqueKeywordToken != null);

                    break;
                case TypeParameterSyntax { NameToken: var name }:
                    Classify(name, SyntaxClassification.TypeParameterName, discard: name.Text.StartsWith('_'));

                    break;
                case ConstantDeclarationSyntax constDecl:
                    Classify(
                        constDecl.NameToken,
                        SyntaxClassification.ConstantName,
                        @public: constDecl.PubKeywordToken != null);

                    break;
                case FunctionDeclarationSyntax fnDecl:
                    Classify(fnDecl.ErrKeywordToken, SyntaxClassification.DeclarationKeyword);
                    Classify(fnDecl.FnKeywordToken, SyntaxClassification.DeclarationKeyword);
                    Classify(
                        fnDecl.NameToken,
                        SyntaxClassification.FunctionName,
                        @public: fnDecl.PubKeywordToken != null,
                        external: fnDecl.ExtKeywordToken != null,
                        fallible: fnDecl.ErrKeywordToken != null);

                    break;
                case TestDeclarationSyntax testDecl:
                    Classify(testDecl.NameToken, SyntaxClassification.TestName);

                    break;
                case AnyTypeSyntax anyType:
                    Classify(anyType.AnyKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case NominalTypeSyntax nomType:
                    Classify(nomType.NameToken, SyntaxClassification.TypeName);

                    break;
                case UnknownTypeSyntax unkType:
                    Classify(unkType.UnkKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case BooleanTypeSyntax boolType:
                    Classify(boolType.BoolKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case IntegerTypeSyntax intType:
                    Classify(intType.IntKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case RealTypeSyntax realType:
                    Classify(realType.RealKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case AtomTypeSyntax atomType:
                    Classify(atomType.AtomKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case StringTypeSyntax strType:
                    Classify(strType.StrKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case ReferenceTypeSyntax refType:
                    Classify(refType.RefKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case HandleTypeSyntax handleType:
                    Classify(handleType.HandleKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case ModuleTypeSyntax modType:
                    Classify(modType.ModKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case AggregateTypeFieldSyntax aggrTypeField:
                    Classify(
                        aggrTypeField.NameToken,
                        SyntaxClassification.FieldName,
                        mutable: aggrTypeField.MutKeywordToken != null);

                    break;
                case ErrorTypeSyntax errType:
                    Classify(errType.ErrKeywordToken, SyntaxClassification.TypeKeyword);
                    Classify(errType.NameToken, SyntaxClassification.ErrorName);

                    break;
                case AgentTypeSyntax agentType:
                    Classify(agentType.AgentKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case AgentTypeMessageSyntax agentTypeMsg:
                    Classify(agentTypeMsg.NameToken, SyntaxClassification.MessageName);

                    break;
                case FunctionTypeSyntax fnType:
                    Classify(fnType.ErrKeywordToken, SyntaxClassification.TypeKeyword);
                    Classify(fnType.FnKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case NoneReturnTypeSyntax noneRetType:
                    Classify(noneRetType.NoneKeywordToken, SyntaxClassification.TypeKeyword);

                    break;
                case AggregateExpressionFieldSyntax aggrExprField:
                    Classify(
                        aggrExprField.NameToken,
                        SyntaxClassification.FieldName,
                        mutable: aggrExprField.MutKeywordToken != null);

                    break;
                case ErrorExpressionSyntax errExpr:
                    Classify(errExpr.ErrKeywordToken, SyntaxClassification.OperationKeyword);
                    Classify(errExpr.NameToken, SyntaxClassification.ErrorName);

                    break;
                case LambdaExpressionSyntax lambdaExpr:
                    Classify(lambdaExpr.ErrKeywordToken, SyntaxClassification.OperationKeyword);
                    Classify(lambdaExpr.FnKeywordToken, SyntaxClassification.OperationKeyword);

                    break;
                case ReceiveExpressionArmSyntax recvExprArm:
                    Classify(recvExprArm.NameToken, SyntaxClassification.MessageName);

                    break;
                case IdentifierExpressionSyntax identExpr when identifiers:
                    Classify(identExpr.NameToken, SyntaxClassification.UnresolvedName);

                    break;
                case FieldExpressionSyntax fieldExpr:
                    Classify(fieldExpr.NameToken, SyntaxClassification.FieldName);

                    break;
                case SendExpressionSyntax sendExpr:
                    Classify(sendExpr.NameToken, SyntaxClassification.MessageName);

                    break;
                case VariableBindingSyntax varBind:
                    Classify(
                        varBind.NameToken, SyntaxClassification.VariableName, mutable: varBind.MutKeywordToken != null);

                    break;
                case DiscardBindingSyntax discardBind:
                    Classify(discardBind.NameToken, SyntaxClassification.VariableName, discard: true);

                    break;
                case AggregatePatternFieldSyntax aggrPatField:
                    Classify(aggrPatField.NameToken, SyntaxClassification.FieldName);

                    break;
                case ErrorPatternSyntax errPat:
                    Classify(errPat.ErrKeywordToken, SyntaxClassification.OperationKeyword);
                    Classify(errPat.NameToken, SyntaxClassification.ErrorName);

                    break;
            }
        }
    }

    private static void ClassifySemanticNodes(
        SemanticNode node, SourceTextSpan span, List<ClassifiedSourceTextSpan> classifications)
    {
        foreach (var child in node.DescendantsAndSelf().OfType<IdentifierExpressionSemantics>())
        {
            var ident = child.Syntax.NameToken;

            if (ident.IsMissing || !span.Overlaps(ident.Span))
                continue;

            if (child.Symbol is not { } sym)
            {
                classifications.Add(
                    new(ident.Span, SyntaxClassification.UnresolvedName, SyntaxClassificationModifiers.None));

                continue;
            }

            static SyntaxClassification ClassifySymbol(
                Symbol symbol,
                out bool @public,
                out bool external,
                out bool fallible,
                out bool mutable,
                out bool discard,
                out bool upvalue)
            {
                SyntaxClassification classification;

                if (symbol is UpvalueSymbol up)
                {
                    classification = ClassifySymbol(
                        up.Parent, out @public, out external, out fallible, out _, out discard, out _);

                    mutable = false;
                    upvalue = true;

                    return classification;
                }

                (classification, @public, external, fallible) = symbol switch
                {
                    DeclarationSymbol => symbol.Bindings[0] switch
                    {
                        ConstantDeclarationSemantics cons =>
                            (SyntaxClassification.ConstantName, cons.IsPublic, false, false),
                        FunctionDeclarationSemantics fn =>
                            (SyntaxClassification.FunctionName, fn.IsPublic, fn.IsExternal, fn.IsFallible),
                        TestDeclarationSemantics test => (SyntaxClassification.TestName, false, false, false),
                        _ => throw new UnreachableException(),
                    },
                    ParameterSymbol parameter => (SyntaxClassification.CodeParameterName, false, false, false),
                    VariableSymbol variable => (SyntaxClassification.VariableName, false, false, false),
                    InteractiveSymbol interactive => (SyntaxClassification.InteractiveName, false, false, false),
                    _ => throw new UnreachableException(),
                };

                mutable = symbol.IsMutable;
                discard = symbol.IsDiscard;
                upvalue = false;

                return classification;
            }

            var cls = ClassifySymbol(sym, out var pub, out var ext, out var err, out var mut, out var disc, out var up);
            var modifiers = SyntaxClassificationModifiers.None;

            if (pub)
                modifiers |= SyntaxClassificationModifiers.Public;

            if (ext)
                modifiers |= SyntaxClassificationModifiers.External;

            if (err)
                modifiers |= SyntaxClassificationModifiers.Fallible;

            if (mut)
                modifiers |= SyntaxClassificationModifiers.Mutable;

            if (disc)
                modifiers |= SyntaxClassificationModifiers.Discard;

            if (up)
                modifiers |= SyntaxClassificationModifiers.Upvalue;

            classifications.Add(new(ident.Span, cls, modifiers));
        }
    }
}
