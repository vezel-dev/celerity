using Vezel.Celerity.Language.Semantics.Binding;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics;

internal sealed class LanguageAnalyzer
{
    internal sealed class AnalysisVisitor : SyntaxVisitor<SemanticNode>
    {
        private readonly struct ScopeContext<T> : IDisposable
            where T : Scope, IScope<T>
        {
            public T Scope { get; }

            private readonly AnalysisVisitor _visitor;

            public ScopeContext(AnalysisVisitor visitor)
            {
                _visitor = visitor;
                _visitor._scope = Scope = T.Create(visitor._scope);
            }

            public void Dispose()
            {
                _visitor._scope = _visitor._scope.Parent!;
            }
        }

        private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

        private readonly Dictionary<string, (List<UseDeclarationSemantics> Declarations, ModulePath? Path)> _uses =
            new();

        private readonly List<Symbol> _duplicates = new();

        private Scope _scope = Scope.Create(null);

        public AnalysisVisitor(ImmutableArray<SourceDiagnostic>.Builder diagnostics)
        {
            _diagnostics = diagnostics;
        }

        public DocumentSemantics Analyze(DocumentSyntax syntax)
        {
            var semantics = VisitDocument(syntax);

            foreach (var (name, (decls, _)) in _uses)
                if (decls.Count != 1)
                    Error(
                        StandardDiagnosticCodes.DuplicateUseDeclaration,
                        decls[0].Syntax.NameToken.GetLocation(),
                        $"Multiple 'use' declarations for '{name}' in module",
                        decls.Skip(1).Select(
                            static decl => (decl.Syntax.NameToken.GetLocation(), "Also declared here")));

            foreach (var sym in _duplicates)
            {
                var (code, msg, note) = sym switch
                {
                    DeclarationSymbol =>
                        (StandardDiagnosticCodes.DuplicateCodeDeclaration,
                         $"Multiple declarations of '{sym.Name}' in the same module",
                         "Also declared here"),
                    ParameterSymbol =>
                        (StandardDiagnosticCodes.DuplicateParameterBinding,
                         $"Multiple bindings of parameter '{sym.Name}' in the same function",
                         "Also bound here"),
                    VariableSymbol =>
                        (StandardDiagnosticCodes.DuplicateVariableBinding,
                         $"Multiple bindings of variable '{sym.Name}' in the same pattern context",
                         "Also bound here"),
                    _ => throw new UnreachableException(),
                };

                var locs = sym.GetLocations().ToArray();

                Error(code, locs[0], msg, locs.Skip(1).Select(loc => (loc, note)));
            }

            return semantics;
        }

        private ScopeContext<T> PushScope<T>()
            where T : Scope, IScope<T>
        {
            return new(this);
        }

        private void Error(
            SourceDiagnosticCode code,
            SourceLocation location,
            string message)
        {
            Error(code, location, message, Array.Empty<(SourceLocation, string)>());
        }

        private void Error(
            SourceDiagnosticCode code,
            SourceLocation location,
            string message,
            IEnumerable<(SourceLocation Location, string Message)> notes)
        {
            _diagnostics.Add(
                SourceDiagnostic.Create(
                    code,
                    SourceDiagnosticSeverity.Error,
                    location,
                    message,
                    notes.Select(static t => SourceDiagnosticNote.Create(t.Location, t.Message))));
        }

        private static ModulePath? CreateModulePath(ModulePathSyntax path)
        {
            return SanitizeModulePath(path) is { Length: not 0 } comps ? new(comps) : null;
        }

        private (UseDeclarationSemantics? Use, ModulePath? Path) ResolveModulePath(ModulePathSyntax path)
        {
            var comps = SanitizeModulePath(path);

            return comps.Length == 1 && _uses.TryGetValue(comps[0], out var tup)
                ? (tup.Declarations[0], tup.Path)
                : comps.Length != 0
                    ? (null, new(comps))
                    : (null, null);
        }

        private static string[] SanitizeModulePath(ModulePathSyntax path)
        {
            return path
                .ComponentTokens
                .Elements
                .Where(static t => !t.IsMissing)
                .Select(static t => t.Text)
                .ToArray();
        }

        private static ImmutableArray<T>.Builder Builder<T>(int capacity)
            where T : SemanticNode
        {
            return ImmutableArray.CreateBuilder<T>(capacity);
        }

        private static SemanticNodeList<T> List<T>(ImmutableArray<T>.Builder elements)
            where T : SemanticNode
        {
            return new(elements.DrainToImmutable());
        }

        private SemanticNodeList<TTo> ConvertList<TFrom, TTo>(
            SyntaxItemList<TFrom> list,
            Func<AnalysisVisitor, TFrom, TTo> converter,
            Func<TFrom, bool>? predicate = null)
            where TFrom : SyntaxNode
            where TTo : SemanticNode
        {
            if (list.Count == 0)
                return SemanticNodeList<TTo>.Empty;

            predicate ??= _ => true;

            var builder = Builder<TTo>(list.Count);

            foreach (var node in list)
                if (predicate(node))
                    builder.Add(converter(this, node));

            return List(builder);
        }

        private void CheckDuplicateFields<T>(
            SemanticNodeList<T> fields, Func<T, SyntaxToken> selector, string type, string message)
            where T : SemanticNode
        {
            if (fields.Count == 0)
                return;

            var map = new Dictionary<string, List<SourceLocation>>(fields.Count);

            foreach (var field in fields)
            {
                if (selector(field) is not { IsMissing: false } name)
                    continue;

                ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(map, name.Text, out _);

                entry ??= new(1);

                entry.Add(name.GetLocation());
            }

            var note = $"Also {message} here";

            foreach (var (name, locs) in map)
                if (locs.Count != 1)
                    Error(
                        StandardDiagnosticCodes.DuplicateAggregateExpressionField,
                        locs[0],
                        $"{type} field '{name}' is {message} multiple times",
                        locs.Skip(1).Select(loc => (loc, note)));
        }

        // Document

        public DocumentSemantics VisitDocument(DocumentSyntax node)
        {
            return Unsafe.As<DocumentSemantics>(Visit(node)!);
        }

        public override ModuleDocumentSemantics VisitModuleDocument(ModuleDocumentSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var path = CreateModulePath(node.Path);

            // Consider:
            //
            // fn foo() {
            //     bar();
            // }
            //
            // fn bar() {
            //     42;
            // }
            //
            // For this to work, we need to register all declaration symbols before actually analyzing their bodies.
            foreach (var decl in node.Declarations)
                if (decl is CodeDeclarationSyntax { NameToken: { IsMissing: false } name })
                    if (!_scope.DefineSymbol<DeclarationSymbol>(name.Text, out var sym))
                        _duplicates.Add(sym);

            var decls = ConvertList(
                node.Declarations,
                static (@this, decl) => @this.VisitDeclaration(decl),
                static decl => decl is UseDeclarationSyntax or CodeDeclarationSyntax);

            return new(node, attrs, path, decls);
        }

        public override InteractiveDocumentSemantics VisitInteractiveDocument(InteractiveDocumentSyntax node)
        {
            var subs = ConvertList(
                node.Submissions,
                static (@this, sub) => @this.VisitSubmission(sub),
                static sub => sub is not StatementSubmissionSyntax { Statement: MissingStatementSyntax });

            return new(node, subs);
        }

        // Miscellaneous

        public override AttributeSemantics VisitAttribute(AttributeSyntax node)
        {
            return new(node);
        }

        // Submissions

        public SubmissionSemantics VisitSubmission(SubmissionSyntax node)
        {
            return Unsafe.As<SubmissionSemantics>(Visit(node)!);
        }

        public override DeclarationSubmissionSemantics VisitDeclarationSubmission(DeclarationSubmissionSyntax node)
        {
            return new(node, VisitDeclaration(node.Declaration));
        }

        public override StatementSubmissionSemantics VisitStatementSubmission(StatementSubmissionSyntax node)
        {
            return new(node, VisitStatement(node.Statement));
        }

        // Declarations

        public DeclarationSemantics VisitDeclaration(DeclarationSyntax node)
        {
            return Unsafe.As<DeclarationSemantics>(Visit(node)!);
        }

        public override UseDeclarationSemantics VisitUseDeclaration(UseDeclarationSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var (use, path) = ResolveModulePath(node.Path);
            var sema = new UseDeclarationSemantics(node, attrs, use, path);

            if (node.NameToken is { IsMissing: false } name)
            {
                ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_uses, name.Text, out var exists);

                if (!exists)
                    entry = (new(1), path);

                entry.Declarations.Add(sema);
            }

            return sema;
        }

        public override ConstantDeclarationSemantics VisitConstantDeclaration(ConstantDeclarationSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            using var ctx = PushScope<Scope>();

            var body = VisitExpression(node.Body);
            var sema = new ConstantDeclarationSemantics(node, attrs, sym, body);

            sym?.AddBinding(sema);

            return sema;
        }

        public override FunctionDeclarationSemantics VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            using var ctx = PushScope<Scope>();

            var parms = ConvertList(
                node.ParameterList.Parameters.Elements, static (@this, param) => @this.VisitFunctionParameter(param));
            var body = node.Body is { } b ? VisitBlockExpression(b) : null;
            var sema = new FunctionDeclarationSemantics(node, attrs, sym, parms, body);

            sym?.AddBinding(sema);

            return sema;
        }

        public override FunctionParameterSemantics VisitFunctionParameter(FunctionParameterSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var sym = default(ParameterSymbol);

            if (node.NameToken is { IsMissing: false } name)
            {
                if (!_scope.DefineSymbol<ParameterSymbol>(name.Text, out var sym2))
                    _duplicates.Add(sym2);

                sym = Unsafe.As<ParameterSymbol>(sym2); // Only parameters will be defined at this stage.
            }

            var sema = new FunctionParameterSemantics(node, attrs, sym);

            sym?.AddBinding(sema);

            return sema;
        }

        public override TestDeclarationSemantics VisitTestDeclaration(TestDeclarationSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            using var ctx = PushScope<Scope>();

            var body = VisitBlockExpression(node.Body);
            var sema = new TestDeclarationSemantics(node, attrs, sym, body);

            sym?.AddBinding(sema);

            return sema;
        }

        // Statements

        public StatementSemantics VisitStatement(StatementSyntax node)
        {
            return Unsafe.As<StatementSemantics>(Visit(node)!);
        }

        public override LetStatementSemantics VisitLetStatement(LetStatementSyntax node)
        {
            using var ctx = PushScope<Scope>();

            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));

            // We visit the initializer first so that it cannot refer to variables bound in the pattern, as in:
            //
            // let x = x;
            var init = VisitExpression(node.Initializer);
            var pat = VisitPattern(node.Pattern);

            return new(node, attrs, pat, init);
        }

        public override DeferStatementSemantics VisitDeferStatement(DeferStatementSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var expr = VisitExpression(node.Expression);

            return new(node, attrs, expr);
        }

        public override AssertStatementSemantics VisitAssertStatement(AssertStatementSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var expr = VisitExpression(node.Expression);

            return new(node, attrs, expr);
        }

        public override ExpressionStatementSemantics VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var expr = VisitExpression(node.Expression);

            return new(node, attrs, expr);
        }

        // Expressions

        public ExpressionSemantics VisitExpression(ExpressionSyntax node)
        {
            return Unsafe.As<ExpressionSemantics>(Visit(node)!);
        }

        public override LiteralExpressionSemantics VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return new(node);
        }

        public override ModuleExpressionSemantics VisitModuleExpression(ModuleExpressionSyntax node)
        {
            var (use, path) = ResolveModulePath(node.Path);

            return new(node, use, path!);
        }

        public override AggregateExpressionFieldSemantics VisitAggregateExpressionField(
            AggregateExpressionFieldSyntax node)
        {
            return new(node, VisitExpression(node.Value));
        }

        public override RecordExpressionSemantics VisitRecordExpression(RecordExpressionSyntax node)
        {
            var fields = ConvertList(
                node.Fields.Elements, static (@this, field) => @this.VisitAggregateExpressionField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Record", "assigned");

            return new(node, node.With?.Operand is { } with ? VisitExpression(with) : null, fields);
        }

        public override ErrorExpressionSemantics VisitErrorExpression(ErrorExpressionSyntax node)
        {
            var fields = ConvertList(
                node.Fields.Elements, static (@this, field) => @this.VisitAggregateExpressionField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Error", "assigned");

            return new(node, node.With?.Operand is { } with ? VisitExpression(with) : null, fields);
        }

        public override TupleExpressionSemantics VisitTupleExpression(TupleExpressionSyntax node)
        {
            var comps = ConvertList(node.Components.Elements, static (@this, comp) => @this.VisitExpression(comp));

            return new(node, comps);
        }

        public override ArrayExpressionSemantics VisitArrayExpression(ArrayExpressionSyntax node)
        {
            var elems = ConvertList(node.Elements.Elements, static (@this, elem) => @this.VisitExpression(elem));

            return new(node, elems);
        }

        public override SetExpressionSemantics VisitSetExpression(SetExpressionSyntax node)
        {
            var elems = ConvertList(node.Elements.Elements, static (@this, elem) => @this.VisitExpression(elem));

            return new(node, elems);
        }

        public override MapExpressionSemantics VisitMapExpression(MapExpressionSyntax node)
        {
            var pairs = ConvertList(node.Pairs.Elements, static (@this, pair) => @this.VisitMapExpressionPair(pair));

            return new(node, pairs);
        }

        public override MapExpressionPairSemantics VisitMapExpressionPair(MapExpressionPairSyntax node)
        {
            var key = VisitExpression(node.Key);
            var value = VisitExpression(node.Value);

            return new(node, key, value);
        }

        public override LambdaExpressionSemantics VisitLambdaExpression(LambdaExpressionSyntax node)
        {
            using var ctx = PushScope<LambdaScope>();

            var parms = ConvertList(
                node.ParameterList.Parameters.Elements, static (@this, param) => @this.VisitLambdaParameter(param));
            var body = VisitExpression(node.Body);

            // TODO: Should we attach the upvalues array?
            return new(node, parms, body);
        }

        public override LambdaParameterSemantics VisitLambdaParameter(LambdaParameterSyntax node)
        {
            var attrs = ConvertList(node.Attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var sym = default(ParameterSymbol);

            if (node.NameToken is { IsMissing: false } name)
            {
                if (!_scope.DefineSymbol<ParameterSymbol>(name.Text, out var sym2))
                    _duplicates.Add(sym2);

                sym = Unsafe.As<ParameterSymbol>(sym2); // Only parameters will be defined at this stage.
            }

            var sema = new LambdaParameterSemantics(node, attrs, sym);

            sym?.AddBinding(sema);

            return sema;
        }

        public override UnaryExpressionSemantics VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);

            return new(node, oper);
        }

        public override AdditiveExpressionSemantics VisitAdditiveExpression(AdditiveExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override MultiplicativeExpressionSemantics VisitMultiplicativeExpression(
            MultiplicativeExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override BitwiseExpressionSemantics VisitBitwiseExpression(BitwiseExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override ShiftExpressionSemantics VisitShiftExpression(ShiftExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override LogicalExpressionSemantics VisitLogicalExpression(LogicalExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override RelationalExpressionSemantics VisitRelationalExpression(RelationalExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);

            return new(node, left, right);
        }

        public override IfExpressionSemantics VisitIfExpression(IfExpressionSyntax node)
        {
            var cond = VisitExpression(node.Condition);
            var body = VisitBlockExpression(node.Body);
            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;

            return new(node, cond, body, @else);
        }

        public override ConditionExpressionSemantics VisitConditionExpression(ConditionExpressionSyntax node)
        {
            var arms = ConvertList(node.Arms.Elements, static (@this, arm) => @this.VisitConditionExpressionArm(arm));

            return new(node, arms);
        }

        public override ConditionExpressionArmSemantics VisitConditionExpressionArm(ConditionExpressionArmSyntax node)
        {
            var cond = VisitExpression(node.Condition);
            var body = VisitExpression(node.Body);

            return new(node, cond, body);
        }

        public override MatchExpressionSemantics VisitMatchExpression(MatchExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);
            var arms = ConvertList(node.Arms.Elements, static (@this, arm) => @this.VisitExpressionPatternArm(arm));

            return new(node, oper, arms);
        }

        public override ExpressionPatternArmSemantics VisitExpressionPatternArm(ExpressionPatternArmSyntax node)
        {
            using var ctx = PushScope<Scope>();

            var pat = VisitPattern(node.Pattern);
            var guard = node.Guard is { } g ? VisitExpression(g.Condition) : null;
            var body = VisitExpression(node.Body);

            return new(node, pat, guard, body);
        }

        public override ReceiveExpressionSemantics VisitReceiveExpression(ReceiveExpressionSyntax node)
        {
            var arms = ConvertList(node.Arms.Elements, static (@this, arm) => @this.VisitReceiveExpressionArm(arm));
            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;

            return new(node, arms, @else);
        }

        public override ReceiveExpressionArmSemantics VisitReceiveExpressionArm(ReceiveExpressionArmSyntax node)
        {
            using var ctx = PushScope<Scope>();

            var parms = ConvertList(
                node.ParameterList.Parameters.Elements, static (@this, param) => @this.VisitPattern(param.Pattern));
            var guard = node.Guard is { } g ? VisitExpression(g.Condition) : null;
            var body = VisitExpression(node.Body);

            return new(node, parms, guard, body);
        }

        public override TryExpressionSemantics VisitTryExpression(TryExpressionSyntax node)
        {
            using var ctx = PushScope<TryScope>();

            var body = VisitExpression(node.Body);
            var arms = ConvertList(node.Arms.Elements, static (@this, arm) => @this.VisitExpressionPatternArm(arm));
            var calls = ctx.Scope.Calls.DrainToImmutable();
            var raises = ctx.Scope.Raises.DrainToImmutable();
            var sema = new TryExpressionSemantics(node, body, arms, calls, raises);

            foreach (var call in calls)
                call.Try = sema;

            foreach (var raise in raises)
                raise.Try = sema;

            return sema;
        }

        public override WhileExpressionSemantics VisitWhileExpression(WhileExpressionSyntax node)
        {
            using var ctx = PushScope<LoopScope>();

            var cond = VisitExpression(node.Condition);
            var body = VisitBlockExpression(node.Body);
            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;
            var branches = ctx.Scope.Branches.DrainToImmutable();
            var sema = new WhileExpressionSemantics(node, cond, body, @else, branches);

            foreach (var branch in branches)
                branch.Loop = sema;

            return sema;
        }

        public override ForExpressionSemantics VisitForExpression(ForExpressionSyntax node)
        {
            using var ctx = PushScope<LoopScope>();

            // We visit the collection first so that it cannot refer to variables bound in the pattern, as in:
            //
            // for x in x {
            //     42;
            // };
            var collection = VisitExpression(node.Collection);
            var pat = VisitPattern(node.Pattern);
            var body = VisitBlockExpression(node.Body);
            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;
            var branches = ctx.Scope.Branches.DrainToImmutable();
            var sema = new ForExpressionSemantics(node, pat, collection, body, @else, branches);

            foreach (var branch in branches)
                branch.Loop = sema;

            return sema;
        }

        public override ReturnExpressionSemantics VisitReturnExpression(ReturnExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);
            var defers = _scope.CollectDefers(null);

            return new(node, oper, defers);
        }

        public override RaiseExpressionSemantics VisitRaiseExpression(RaiseExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);
            var defers = _scope.CollectDefers(null);
            var sema = new RaiseExpressionSemantics(node, oper, defers);

            _scope.GetEnclosingTry()?.Raises.Add(sema);

            return sema;
        }

        public override NextExpressionSemantics VisitNextExpression(NextExpressionSyntax node)
        {
            var loop = _scope.GetEnclosingLoop();
            var defers = _scope.CollectDefers(loop);
            var sema = new NextExpressionSemantics(node, defers);

            if (loop != null)
                loop.Branches.Add(sema);
            else
                Error(
                    StandardDiagnosticCodes.MissingEnclosingLoop,
                    node.NextKeywordToken.GetLocation(),
                    "No enclosing 'while' or 'for' expression for this 'next' expression");

            return sema;
        }

        public override BreakExpressionSemantics VisitBreakExpression(BreakExpressionSyntax node)
        {
            var result = node.Result is { } r ? VisitExpression(r.Value) : null;
            var loop = _scope.GetEnclosingLoop();
            var defers = _scope.CollectDefers(loop);
            var sema = new BreakExpressionSemantics(node, result, defers);

            if (loop != null)
                loop.Branches.Add(sema);
            else
                Error(
                    StandardDiagnosticCodes.MissingEnclosingLoop,
                    node.BreakKeywordToken.GetLocation(),
                    "No enclosing 'while' or 'for' expression for this 'break' expression");

            return sema;
        }

        public override ParenthesizedExpressionSemantics VisitParenthesizedExpression(
            ParenthesizedExpressionSyntax node)
        {
            return new(node, VisitExpression(node.Expression));
        }

        public override BlockExpressionSemantics VisitBlockExpression(BlockExpressionSyntax node)
        {
            using var ctx = PushScope<BlockScope>();

            var stmts = Builder<StatementSemantics>(node.Statements.Count);

            // Let statements are somewhat special in that they introduce a 'horizontal' scope in the tree; that is,
            // bindings in a let statement become available to siblings to the right of the let statement.
            var lets = new List<ScopeContext<Scope>>();
            var defers = ctx.Scope.Defers;

            foreach (var stmt in node.Statements)
            {
                if (stmt is MissingStatementSyntax)
                    continue;

                if (stmt is LetStatementSyntax)
                    lets.Add(PushScope<Scope>());

                var sema = VisitStatement(stmt);

                if (sema is DeferStatementSemantics defer)
                    defers.Add(defer);

                stmts.Add(sema);
            }

            for (var i = lets.Count - 1; i >= 0; i--)
                lets[i].Dispose();

            defers.Reverse();

            return new(node, List(stmts), defers.DrainToImmutable());
        }

        public override IdentifierExpressionSemantics VisitIdentifierExpression(IdentifierExpressionSyntax node)
        {
            var sym = default(Symbol);

            if (node.IdentifierToken is { IsMissing: false } ident)
            {
                sym = _scope.ResolveSymbol(ident.Text);

                if (sym == null)
                    Error(
                        StandardDiagnosticCodes.UnresolvedIdentifier,
                        ident.GetLocation(),
                        $"Unknown symbol name '{ident.Text}'");
                else if (sym.Bindings.Any(node => node is TestDeclarationSemantics))
                    Error(
                        StandardDiagnosticCodes.IllegalTestReference,
                        ident.GetLocation(),
                        $"Reference to test declaration '{ident.Text}' is illegal");
            }

            return new(node, sym);
        }

        public override AssignmentExpressionSemantics VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var left = VisitExpression(node.LeftOperand);
            var right = VisitExpression(node.RightOperand);
            var sym = default(Symbol);

            switch (left)
            {
                case IdentifierExpressionSemantics ident:
                    if ((sym = ident.Symbol) is { IsMutable: false })
                        Error(
                            StandardDiagnosticCodes.ImmutableAssignmentTarget,
                            node.LeftOperand.GetLocation(),
                            $"Assignment to immutable symbol '{sym.Name}'",
                            sym.GetLocations().Select(static loc => (loc, "Symbol defined here")));

                    break;
                case FieldExpressionSemantics or IndexExpressionSemantics:
                    break;
                default:
                    Error(
                        StandardDiagnosticCodes.InvalidAssignmentTarget,
                        node.LeftOperand.GetLocation(),
                        "Assignment target must be an identifier, field, or index expression");
                    break;
            }

            var sema = new AssignmentExpressionSemantics(node, left, right);

            sym?.AddAssignment(sema);

            return sema;
        }

        public override FieldExpressionSemantics VisitFieldExpression(FieldExpressionSyntax node)
        {
            return new(node, VisitExpression(node.Subject));
        }

        public override IndexExpressionSemantics VisitIndexExpression(IndexExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(
                node.ArgumentList.Arguments.Elements, static (@this, arg) => @this.VisitExpression(arg));

            return new(node, subject, args);
        }

        public override CallExpressionSemantics VisitCallExpression(CallExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(
                node.ArgumentList.Arguments.Elements, static (@this, arg) => @this.VisitExpression(arg));
            var defers = node.QuestionToken != null
                ? _scope.CollectDefers(null)
                : ImmutableArray<DeferStatementSemantics>.Empty;
            var sema = new CallExpressionSemantics(node, subject, args, defers);

            _scope.GetEnclosingTry()?.Calls.Add(sema);

            return sema;
        }

        public override SendExpressionSemantics VisitSendExpression(SendExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(
                node.ArgumentList.Arguments.Elements, static (@this, arg) => @this.VisitExpression(arg));

            return new(node, subject, args);
        }

        // Bindings

        public BindingSemantics VisitBinding(BindingSyntax node)
        {
            return Unsafe.As<BindingSemantics>(Visit(node)!);
        }

        public override VariableBindingSemantics VisitVariableBinding(VariableBindingSyntax node)
        {
            var sym = default(VariableSymbol);

            if (node.NameToken is { IsMissing: false } name)
            {
                if (!_scope.DefineSymbol<ParameterSymbol>(name.Text, out var sym2))
                    _duplicates.Add(sym2);

                sym = Unsafe.As<VariableSymbol>(sym2); // Only variables will be defined at this stage.
            }

            var sema = new VariableBindingSemantics(node, sym);

            sym?.AddBinding(sema);

            return sema;
        }

        public override DiscardBindingSemantics VisitDiscardBinding(DiscardBindingSyntax node)
        {
            if (!_scope.DefineSymbol<ParameterSymbol>(node.NameToken.Text, out var sym))
                _duplicates.Add(sym);

            var sym2 = Unsafe.As<VariableSymbol>(sym); // Only variables will be defined at this stage.
            var sema = new DiscardBindingSemantics(node, sym2);

            sym2?.AddBinding(sema);

            return sema;
        }

        // Patterns

        public PatternSemantics VisitPattern(PatternSyntax node)
        {
            return Unsafe.As<PatternSemantics>(Visit(node)!);
        }

        public override WildcardPatternSemantics VisitWildcardPattern(WildcardPatternSyntax node)
        {
            var binding = VisitBinding(node.Binding);
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, binding, alias);
        }

        public override LiteralPatternSemantics VisitLiteralPattern(LiteralPatternSyntax node)
        {
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, alias);
        }

        public override StringPatternSemantics VisitStringPattern(StringPatternSyntax node)
        {
            var middle = node.MiddleBinding is { } m ? VisitBinding(m) : null;
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, middle, alias);
        }

        public override ModulePatternSemantics VisitModulePattern(ModulePatternSyntax node)
        {
            var (use, path) = ResolveModulePath(node.Path);
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, use, path!, alias);
        }

        public override AggregatePatternFieldSemantics VisitAggregatePatternField(AggregatePatternFieldSyntax node)
        {
            var pat = VisitPattern(node.Pattern);

            return new(node, pat);
        }

        public override RecordPatternSemantics VisitRecordPattern(RecordPatternSyntax node)
        {
            var fields = ConvertList(
                node.Fields.Elements, static (@this, field) => @this.VisitAggregatePatternField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Record", "matched");

            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, fields, alias);
        }

        public override ErrorPatternSemantics VisitErrorPattern(ErrorPatternSyntax node)
        {
            var fields = ConvertList(
                node.Fields.Elements, static (@this, field) => @this.VisitAggregatePatternField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Error", "matched");

            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, fields, alias);
        }

        public override TuplePatternSemantics VisitTuplePattern(TuplePatternSyntax node)
        {
            var comps = ConvertList(node.Components.Elements, static (@this, comp) => @this.VisitPattern(comp));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, comps, alias);
        }

        public override ArrayPatternSemantics VisitArrayPattern(ArrayPatternSyntax node)
        {
            var left = node.LeftClause is { } l ? VisitArrayPatternClause(l) : null;
            var middle = node.MiddleBinding is { } m ? VisitBinding(m) : null;
            var right = node.RightClause is { } r ? VisitArrayPatternClause(r) : null;
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, left, middle, right, alias);
        }

        public override ArrayPatternClauseSemantics VisitArrayPatternClause(ArrayPatternClauseSyntax node)
        {
            var elems = ConvertList(node.Elements.Elements, static (@this, elem) => @this.VisitPattern(elem));

            return new(node, elems);
        }

        public override MapPatternSemantics VisitMapPattern(MapPatternSyntax node)
        {
            var pairs = ConvertList(node.Pairs.Elements, static (@this, pair) => @this.VisitMapPatternPair(pair));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, pairs, alias);
        }

        public override MapPatternPairSemantics VisitMapPatternPair(MapPatternPairSyntax node)
        {
            var key = VisitExpression(node.Key);
            var value = VisitPattern(node.Value);

            return new(node, key, value);
        }

        public override SetPatternSemantics VisitSetPattern(SetPatternSyntax node)
        {
            var elems = ConvertList(node.Elements.Elements, static (@this, elem) => @this.VisitExpression(elem));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, elems, alias);
        }
    }

    private readonly DocumentSyntax _syntax;

    private readonly AnalysisVisitor _walker;

    public LanguageAnalyzer(DocumentSyntax syntax, ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _syntax = syntax;
        _walker = new(diagnostics);
    }

    public DocumentSemantics Analyze()
    {
        return _walker.Analyze(_syntax);
    }
}
