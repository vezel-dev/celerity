// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Diagnostics;
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

        private readonly SyntaxTree _tree;

        private readonly InteractiveContext _context;

        private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

        private readonly Dictionary<string, (List<UseDeclarationSemantics> Declarations, ModulePath? Path)> _uses = [];

        private readonly List<IdentifierExpressionSemantics> _identifiers = [];

        private readonly HashSet<Symbol> _duplicates = [];

        private Scope _scope = new(parent: null);

        public AnalysisVisitor(
            SyntaxTree tree, InteractiveContext context, ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            _tree = tree;
            _context = context;
            _diagnostics = diagnostics;
        }

        public DocumentSemantics Analyze()
        {
            var semantics = VisitDocument(_tree.Root);

            if (semantics is ModuleDocumentSemantics)
                foreach (var (name, (decls, _)) in _uses.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
                    if (decls.Count != 1)
                        Error(
                            decls[0].Syntax.NameToken.Span,
                            StandardDiagnosticCodes.DuplicateUseDeclaration,
                            $"Multiple 'use' declarations for '{name}' in module",
                            decls.Skip(1).Select(static decl => (decl.Syntax.NameToken.Span, "Also declared here")));

            foreach (var sym in _duplicates.OrderBy(static sym => sym.Name))
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

                var spans = sym.GetSpans().ToArray();

                Error(spans[0], code, msg, spans.Skip(1).Select(span => (span, note)));
            }

            foreach (var ident in _identifiers)
            {
                var sym = ident.Symbol!;

                if (!sym.Bindings.Any(static node => node is TestDeclarationSemantics))
                    continue;

                var name = ident.Syntax.NameToken;

                Error(
                    name.Span,
                    StandardDiagnosticCodes.IllegalTestReference,
                    $"Reference to test declaration '{name.Text}' is illegal",
                    sym.GetSpans().Select(static span => (span, "Symbol declared here")));
            }

            return semantics;
        }

        private ScopeContext<T> PushScope<T>(out T scope)
            where T : Scope, IScope<T>
        {
            var ctx = new ScopeContext<T>(this);

            scope = ctx.Scope;

            return ctx;
        }

        private void Error(
            SourceTextSpan span,
            DiagnosticCode code,
            string message)
        {
            Error(span, code, message, notes: []);
        }

        private void Error(
            SourceTextSpan span,
            DiagnosticCode code,
            string message,
            IEnumerable<(SourceTextSpan Span, string Message)> notes)
        {
            _diagnostics.Add(
                new(
                    _tree,
                    span,
                    DiagnosticSeverity.Error,
                    code,
                    message,
                    [.. notes.Select(static t => new DiagnosticNote(t.Span, t.Message))]));
        }

        private (UseDeclarationSemantics? Use, ModulePath? Path) ResolveModulePath(ModulePathSyntax path)
        {
            var comps = path
                .ComponentTokens
                .Elements
                .Where(static t => !t.IsMissing)
                .Select(static t => t.Text)
                .ToArray();

            return comps.Length switch
            {
                1 when _uses.TryGetValue(comps[0], out var t) => (t.Declarations[^1], t.Path),
                1 when _context.TryGetUse(comps[0], out var p) => (null, p),
                _ when ModulePath.TryCreate(comps, out var p) => (null, p),
                _ => (null, null),
            };
        }

        private static ImmutableArray<T>.Builder Builder<T>(int capacity)
            where T : SemanticNode
        {
            return ImmutableArray.CreateBuilder<T>(capacity);
        }

        private static SemanticNodeList<TSemantics, TSyntax> List<TSemantics, TSyntax>(
            SyntaxItemList<TSyntax> syntax, ImmutableArray<TSemantics>.Builder elements)
            where TSemantics : SemanticNode
            where TSyntax : SyntaxNode
        {
            return new(syntax, elements.DrainToImmutable());
        }

        private SemanticNodeList<TSemantics, TSyntax> ConvertList<TSyntax, TSemantics>(
            SyntaxItemList<TSyntax> syntax,
            Func<AnalysisVisitor, TSyntax, TSemantics> converter,
            Func<TSyntax, bool>? predicate = null)
            where TSyntax : SyntaxNode
            where TSemantics : SemanticNode
        {
            if (syntax.Count == 0)
                return new(syntax, []);

            var builder = Builder<TSemantics>(syntax.Count);

            foreach (var node in syntax)
                if (predicate?.Invoke(node) ?? true)
                    builder.Add(converter(this, node));

            return List(syntax, builder);
        }

        private SemanticNodeList<AttributeSemantics, AttributeSyntax> ConvertAttributeList(
            SyntaxNode container, SyntaxItemList<AttributeSyntax> attributes)
        {
            var attrs = ConvertList(attributes, static (@this, attr) => @this.VisitAttribute(attr));
            var grouped = new Dictionary<string, List<AttributeSemantics>>();

            foreach (var attr in attrs)
            {
                // The parser will already have created a diagnostic if the attribute is missing a value; avoid creating
                // additional diagnostics here as it might overwhelm the user.
                if (attr.Value is not { } value)
                    continue;

                var name = attr.Name;

                var (containerValid, valueValid, valueMsg, allowMultiple) = name switch
                {
                    "deprecated" => (container is ModuleDocumentSyntax or
                                                  ConstantDeclarationSyntax or
                                                  FunctionDeclarationSyntax,
                                     value is ReadOnlyMemory<byte>,
                                     "string literal (reason)",
                                     false),
                    "doc" => (container is ModuleDocumentSyntax or
                                           ConstantDeclarationSyntax or
                                           FunctionDeclarationSyntax,
                              value is ReadOnlyMemory<byte> or false,
                              "string literal (documentation text) or 'false' literal",
                              false),
                    "flaky" or "ignore" => (container is TestDeclarationSyntax,
                                            value is ReadOnlyMemory<byte>,
                                            "string literal (reason)",
                                            false),
                    "lint" => (true,
                               value is ReadOnlyMemory<byte> utf8 &&
                               Encoding.UTF8.GetString(utf8.Span).Split(':', StringSplitOptions.RemoveEmptyEntries) is
                                   [var left, var right] &&
                               DiagnosticCode.TryCreate(left, out _) &&
                               right is "none" or "warning" or "error",
                               "string literal of the form '<name>:<severity>'",
                               true),
                    _ => (true, true, null, true),
                };

                if (!containerValid)
                    Error(
                        attr.Syntax.Span,
                        StandardDiagnosticCodes.InvalidStandardAttributeTarget,
                        $"Standard attribute '{name}' is not valid on this item");

                if (!valueValid)
                    Error(
                        attr.Syntax.ValueToken.Span,
                        StandardDiagnosticCodes.InvalidStandardAttributeValue,
                        $"Value for standard attribute '{name}' must be a {valueMsg}");

                if (!allowMultiple)
                    (CollectionsMarshal.GetValueRefOrAddDefault(grouped, name, out _) ??= []).Add(attr);
            }

            foreach (var (name, list) in grouped.OrderBy(static kvp => kvp.Key))
                if (list.Count != 1)
                    Error(
                        list[0].Syntax.Span,
                        StandardDiagnosticCodes.DuplicateStandardAttribute,
                        $"Standard attribute '{name}' specified multiple times",
                        list.Skip(1).Select(static attr => (attr.Syntax.Span, "Also specified here")));

            return attrs;
        }

        private SeparatedSemanticNodeList<TSemantics, TSyntax> ConvertList<TSyntax, TSemantics>(
            SeparatedSyntaxItemList<TSyntax> syntax, Func<AnalysisVisitor, TSyntax, TSemantics> converter)
            where TSyntax : SyntaxNode
            where TSemantics : SemanticNode
        {
            var elements = syntax.Elements;

            if (elements.Length == 0)
                return new(syntax, []);

            var builder = Builder<TSemantics>(elements.Length);

            foreach (var node in elements)
                builder.Add(converter(this, node));

            return new(syntax, builder.DrainToImmutable());
        }

        private void CheckDuplicateFields<TSemantics, TSyntax>(
            SeparatedSemanticNodeList<TSemantics, TSyntax> fields,
            Func<TSemantics, SyntaxToken> selector,
            string type,
            string message)
            where TSemantics : SemanticNode
            where TSyntax : SyntaxNode
        {
            if (fields.Count == 0)
                return;

            var map = new Dictionary<string, List<SourceTextSpan>>(fields.Count);

            foreach (var field in fields)
                if (selector(field) is { IsMissing: false } name)
                    (CollectionsMarshal.GetValueRefOrAddDefault(map, name.Text, out _) ??= []).Add(name.Span);

            foreach (var (name, spans) in map)
                if (spans.Count != 1)
                    Error(
                        spans[0],
                        StandardDiagnosticCodes.DuplicateAggregateExpressionField,
                        $"{type} field '{name}' is {message} multiple times",
                        spans.Skip(1).Select(span => (span, $"Also {message} here")));
        }

        // Document

        public DocumentSemantics VisitDocument(DocumentSyntax node)
        {
            return Unsafe.As<DocumentSemantics>(Visit(node)!);
        }

        public override ModuleDocumentSemantics VisitModuleDocument(ModuleDocumentSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);

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
                        _ = _duplicates.Add(sym);

            var decls = ConvertList(
                node.Declarations,
                static (@this, decl) => @this.VisitDeclaration(decl),
                static decl => decl is UseDeclarationSyntax or CodeDeclarationSyntax);

            return new(node, attrs, decls);
        }

        public override InteractiveDocumentSemantics VisitInteractiveDocument(InteractiveDocumentSyntax node)
        {
            var subs = Builder<SubmissionSemantics>(node.Submissions.Count);

            // See VisitBlockExpression for how this works.
            var lets = new List<ScopeContext<Scope>>();

            foreach (var sub in node.Submissions)
            {
                if (sub is StatementSubmissionSyntax { Statement: LetStatementSyntax })
                    lets.Add(PushScope(out Scope _));

                subs.Add(VisitSubmission(sub));
            }

            for (var i = lets.Count - 1; i >= 0; i--)
                lets[i].Dispose();

            return new(node, List(node.Submissions, subs));
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
            var decl = VisitDeclaration(node.Declaration);

            return new(node, decl);
        }

        public override StatementSubmissionSemantics VisitStatementSubmission(StatementSubmissionSyntax node)
        {
            var stmt = VisitStatement(node.Statement);

            return new(node, stmt);
        }

        // Declarations

        public DeclarationSemantics VisitDeclaration(DeclarationSyntax node)
        {
            return Unsafe.As<DeclarationSemantics>(Visit(node)!);
        }

        public override UseDeclarationSemantics VisitUseDeclaration(UseDeclarationSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var (use, path) = ResolveModulePath(node.Path);

            var sema = new UseDeclarationSemantics(node, attrs, use, path);

            if (node.NameToken is { IsMissing: false } name)
            {
                ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_uses, name.Text, out var exists);

                if (!exists)
                    entry = ([], path);

                entry.Declarations.Add(sema);
            }

            return sema;
        }

        public override ConstantDeclarationSemantics VisitConstantDeclaration(ConstantDeclarationSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            ExpressionSemantics body;

            using (PushScope<Scope>(out _))
                body = VisitExpression(node.Body);

            var sema = new ConstantDeclarationSemantics(node, attrs, sym, body);

            sym?.AddBinding(sema);

            return sema;
        }

        public override FunctionDeclarationSemantics VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            FunctionScope scope;
            SeparatedSemanticNodeList<FunctionParameterSemantics, FunctionParameterSyntax> parms;
            BlockExpressionSemantics? body;

            using (var ctx = PushScope(out scope))
            {
                scope.IsFallible = node.ErrKeywordToken != null;

                parms = ConvertList(
                    node.ParameterList.Parameters, static (@this, param) => @this.VisitFunctionParameter(param));
                body = node.Body is { } b ? VisitBlockExpression(b) : null;
            }

            var branches = scope.BranchExpressions.DrainToImmutable();
            var calls = scope.CallExpressions.DrainToImmutable();

            var sema = new FunctionDeclarationSemantics(node, attrs, sym, parms, body, branches, calls);

            sym?.AddBinding(sema);

            foreach (var branch in branches)
                branch.Function = sema;

            foreach (var call in calls)
                call.Function = sema;

            return sema;
        }

        public override FunctionParameterSemantics VisitFunctionParameter(FunctionParameterSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var sym = default(ParameterSymbol);

            if (node.NameToken is { IsMissing: false } name)
            {
                if (!_scope.DefineSymbol<ParameterSymbol>(name.Text, out var sym2))
                    _ = _duplicates.Add(sym2);

                sym = Unsafe.As<ParameterSymbol>(sym2); // Only parameters will be defined at this stage.
            }

            var sema = new FunctionParameterSemantics(node, attrs, sym);

            sym?.AddBinding(sema);

            return sema;
        }

        public override TestDeclarationSemantics VisitTestDeclaration(TestDeclarationSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var sym = node.NameToken is { IsMissing: false } name
                ? Unsafe.As<DeclarationSymbol>(_scope.ResolveSymbol(name.Text)!)
                : null;

            BlockExpressionSemantics body;

            using (PushScope(out Scope _))
                body = VisitBlockExpression(node.Body);

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
            var attrs = ConvertAttributeList(node, node.Attributes);

            // We visit the initializer first so that it cannot refer to variables bound in the pattern, as in:
            //
            // let x = x;
            var init = VisitExpression(node.Initializer);
            var pat = VisitPattern(node.Pattern);

            return new(node, attrs, pat, init);
        }

        public override DeferStatementSemantics VisitDeferStatement(DeferStatementSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);

            ExpressionSemantics expr;

            using (PushScope(out DeferScope _))
                expr = VisitExpression(node.Expression);

            return new(node, attrs, expr);
        }

        public override ExpressionStatementSemantics VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
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
            var field = VisitExpression(node.Value);

            return new(node, field);
        }

        public override RecordExpressionSemantics VisitRecordExpression(RecordExpressionSyntax node)
        {
            var with = node.With?.Operand is { } w ? VisitExpression(w) : null;
            var fields = ConvertList(node.Fields, static (@this, field) => @this.VisitAggregateExpressionField(field));
            var meta = node.Meta?.Operand is { } m ? VisitExpression(m) : null;

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Record", "assigned");

            return new(node, with, fields, meta);
        }

        public override ErrorExpressionSemantics VisitErrorExpression(ErrorExpressionSyntax node)
        {
            var with = node.With?.Operand is { } w ? VisitExpression(w) : null;
            var fields = ConvertList(node.Fields, static (@this, field) => @this.VisitAggregateExpressionField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Error", "assigned");

            return new(node, with, fields);
        }

        public override TupleExpressionSemantics VisitTupleExpression(TupleExpressionSyntax node)
        {
            var comps = ConvertList(node.Components, static (@this, comp) => @this.VisitExpression(comp));

            return new(node, comps);
        }

        public override ArrayExpressionSemantics VisitArrayExpression(ArrayExpressionSyntax node)
        {
            var elems = ConvertList(node.Elements, static (@this, elem) => @this.VisitExpression(elem));

            return new(node, elems);
        }

        public override SetExpressionSemantics VisitSetExpression(SetExpressionSyntax node)
        {
            var elems = ConvertList(node.Elements, static (@this, elem) => @this.VisitExpression(elem));

            return new(node, elems);
        }

        public override MapExpressionSemantics VisitMapExpression(MapExpressionSyntax node)
        {
            var pairs = ConvertList(node.Pairs, static (@this, pair) => @this.VisitMapExpressionPair(pair));

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
            LambdaScope scope;
            SeparatedSemanticNodeList<LambdaParameterSemantics, LambdaParameterSyntax> parms;
            ExpressionSemantics body;

            using (var ctx = PushScope(out scope))
            {
                scope.IsFallible = node.ErrKeywordToken != null;

                parms = ConvertList(
                    node.ParameterList.Parameters, static (@this, param) => @this.VisitLambdaParameter(param));
                body = VisitExpression(node.Body);
            }

            var upvalues = scope.CollectUpvalues();
            var refs = scope.ThisExpressions.ToImmutable();
            var branches = scope.BranchExpressions.DrainToImmutable();
            var calls = scope.CallExpressions.DrainToImmutable();

            var sema = new LambdaExpressionSemantics(node, parms, body, upvalues, refs, branches, calls);

            foreach (var @this in refs)
                @this.Lambda = sema;

            foreach (var branch in branches)
                branch.Lambda = sema;

            foreach (var call in calls)
                call.Lambda = sema;

            return sema;
        }

        public override LambdaParameterSemantics VisitLambdaParameter(LambdaParameterSyntax node)
        {
            var attrs = ConvertAttributeList(node, node.Attributes);
            var sym = default(ParameterSymbol);

            if (node.NameToken is { IsMissing: false } name)
            {
                if (!_scope.DefineSymbol<ParameterSymbol>(name.Text, out var sym2))
                    _ = _duplicates.Add(sym2);

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
            var arms = ConvertList(node.Arms, static (@this, arm) => @this.VisitConditionExpressionArm(arm));

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
            var arms = ConvertList(node.Arms, static (@this, arm) => @this.VisitExpressionPatternArm(arm));

            return new(node, oper, arms);
        }

        public override ExpressionPatternArmSemantics VisitExpressionPatternArm(ExpressionPatternArmSyntax node)
        {
            PatternSemantics pat;
            ExpressionSemantics? guard;
            ExpressionSemantics body;

            using (PushScope(out Scope _))
            {
                pat = VisitPattern(node.Pattern);
                guard = node.Guard is { } g ? VisitExpression(g.Condition) : null;
                body = VisitExpression(node.Body);
            }

            return new(node, pat, guard, body);
        }

        public override ReceiveExpressionSemantics VisitReceiveExpression(ReceiveExpressionSyntax node)
        {
            var arms = ConvertList(node.Arms, static (@this, arm) => @this.VisitReceiveExpressionArm(arm));
            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;

            return new(node, arms, @else);
        }

        public override ReceiveExpressionArmSemantics VisitReceiveExpressionArm(ReceiveExpressionArmSyntax node)
        {
            SeparatedSemanticNodeList<ReceiveParameterSemantics, ReceiveParameterSyntax> parms;
            ExpressionSemantics? guard;
            ExpressionSemantics body;

            using (PushScope(out Scope _))
            {
                parms = ConvertList(
                    node.ParameterList.Parameters, static (@this, param) => @this.VisitReceiveParameter(param));
                guard = node.Guard is { } g ? VisitExpression(g.Condition) : null;
                body = VisitExpression(node.Body);
            }

            return new(node, parms, guard, body);
        }

        public override ReceiveParameterSemantics VisitReceiveParameter(ReceiveParameterSyntax node)
        {
            var pat = VisitPattern(node.Pattern);

            return new(node, pat);
        }

        public override TryExpressionSemantics VisitTryExpression(TryExpressionSyntax node)
        {
            TryScope scope;
            ExpressionSemantics body;

            using (PushScope(out scope))
                body = VisitExpression(node.Operand);

            var arms = ConvertList(node.Arms, static (@this, arm) => @this.VisitExpressionPatternArm(arm));
            var raises = scope.RaiseExpressions.DrainToImmutable();
            var calls = scope.CallExpressions.DrainToImmutable();

            var sema = new TryExpressionSemantics(node, body, arms, raises, calls);

            foreach (var raise in raises)
                raise.Try = sema;

            foreach (var call in calls)
                call.Try = sema;

            return sema;
        }

        public override WhileExpressionSemantics VisitWhileExpression(WhileExpressionSyntax node)
        {
            LoopScope scope;
            ExpressionSemantics cond;
            BlockExpressionSemantics body;

            using (PushScope(out scope))
            {
                cond = VisitExpression(node.Condition);
                body = VisitBlockExpression(node.Body);
            }

            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;
            var branches = scope.BranchExpressions.DrainToImmutable();

            var sema = new WhileExpressionSemantics(node, cond, body, @else, branches);

            foreach (var branch in branches)
                branch.Loop = sema;

            return sema;
        }

        public override ForExpressionSemantics VisitForExpression(ForExpressionSyntax node)
        {
            LoopScope scope;
            ExpressionSemantics collection;
            PatternSemantics pat;
            BlockExpressionSemantics body;

            using (PushScope(out scope))
            {
                // We visit the collection first so that it cannot refer to variables bound in the pattern, as in:
                //
                // for x in x {
                //     42;
                // };
                collection = VisitExpression(node.Collection);
                pat = VisitPattern(node.Pattern);
                body = VisitBlockExpression(node.Body);
            }

            var @else = node.Else is { } e ? VisitBlockExpression(e.Body) : null;
            var branches = scope.BranchExpressions.DrainToImmutable();

            var sema = new ForExpressionSemantics(node, pat, collection, body, @else, branches);

            foreach (var branch in branches)
                branch.Loop = sema;

            return sema;
        }

        public override ReturnExpressionSemantics VisitReturnExpression(ReturnExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);
            var defers = _scope.CollectDefers(target: null);

            var sema = new ReturnExpressionSemantics(node, oper, defers);

            if (_scope.GetEnclosingFunction(ignoreDefer: false) is { } function)
                function.BranchExpressions.Add(sema);
            else
                Error(
                    node.Span,
                    StandardDiagnosticCodes.MissingEnclosingFunction,
                    "No enclosing function for 'ret' expression");

            if (sema.IsTail && oper is not CallExpressionSemantics)
                Error(
                    node.Span,
                    StandardDiagnosticCodes.ImproperTailCall,
                    "Operand of 'tail ret' expression is not a call expression");

            return sema;
        }

        public override RaiseExpressionSemantics VisitRaiseExpression(RaiseExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);
            var defers = _scope.CollectDefers(target: null);

            var sema = new RaiseExpressionSemantics(node, oper, defers);

            if (_scope.GetEnclosingTry() is { } @try)
                @try.RaiseExpressions.Add(sema);
            else if (_scope.GetEnclosingFunction(ignoreDefer: true) is { IsFallible: true } function)
                function.BranchExpressions.Add(sema);
            else
                Error(
                    node.Span,
                    StandardDiagnosticCodes.ErrorInInfallibleContext,
                    "'raise' expression in infallible context is invalid");

            return sema;
        }

        public override NextExpressionSemantics VisitNextExpression(NextExpressionSyntax node)
        {
            var loop = _scope.GetEnclosingLoop();
            var defers = _scope.CollectDefers(loop);

            var sema = new NextExpressionSemantics(node, defers);

            if (loop != null)
                loop.BranchExpressions.Add(sema);
            else
                Error(
                    node.Span,
                    StandardDiagnosticCodes.MissingEnclosingLoop,
                    "No enclosing 'while' or 'for' expression for 'next' expression");

            return sema;
        }

        public override BreakExpressionSemantics VisitBreakExpression(BreakExpressionSyntax node)
        {
            var result = node.Result is { } r ? VisitExpression(r.Value) : null;
            var loop = _scope.GetEnclosingLoop();
            var defers = _scope.CollectDefers(loop);

            var sema = new BreakExpressionSemantics(node, result, defers);

            if (loop != null)
                loop.BranchExpressions.Add(sema);
            else
                Error(
                    node.Span,
                    StandardDiagnosticCodes.MissingEnclosingLoop,
                    "No enclosing 'while' or 'for' expression for 'break' expression");

            return sema;
        }

        public override ParenthesizedExpressionSemantics VisitParenthesizedExpression(
            ParenthesizedExpressionSyntax node)
        {
            var expr = VisitExpression(node.Expression);

            return new(node, expr);
        }

        public override BlockExpressionSemantics VisitBlockExpression(BlockExpressionSyntax node)
        {
            ImmutableArray<StatementSemantics>.Builder stmts;
            ImmutableArray<DeferStatementSemantics>.Builder defers;

            using (PushScope(out BlockScope scope))
            {
                stmts = Builder<StatementSemantics>(node.Statements.Count);
                defers = scope.DeferStatements;

                // Let statements are somewhat special in that they introduce a 'horizontal' scope in the tree; that is,
                // bindings in a let statement become available to siblings to the right of the let statement.
                var lets = new List<ScopeContext<Scope>>();

                foreach (var stmt in node.Statements)
                {
                    if (stmt is LetStatementSyntax)
                        lets.Add(PushScope(out Scope _));

                    var sema = VisitStatement(stmt);

                    if (sema is DeferStatementSemantics defer)
                        defers.Add(defer);

                    stmts.Add(sema);
                }

                for (var i = lets.Count - 1; i >= 0; i--)
                    lets[i].Dispose();

                defers.Reverse();
            }

            return new(node, List(node.Statements, stmts), defers.DrainToImmutable());
        }

        public override IdentifierExpressionSemantics VisitIdentifierExpression(IdentifierExpressionSyntax node)
        {
            var sym = default(Symbol);

            if (node.NameToken is { IsMissing: false } ident)
            {
                sym = _scope.ResolveSymbol(ident.Text);

                if (sym == null && _context.TryGetSymbol(ident.Text, out var sym2))
                    sym = sym2;

                if (sym == null)
                    Error(
                        ident.Span,
                        StandardDiagnosticCodes.UnresolvedIdentifier,
                        $"Unknown symbol name '{ident.Text}'");
            }

            var sema = new IdentifierExpressionSemantics(node, sym);

            if (sym != null)
            {
                sym.AddReference(sema);
                _identifiers.Add(sema);
            }

            return sema;
        }

        public override ThisExpressionSemantics VisitThisExpression(ThisExpressionSyntax node)
        {
            var sema = new ThisExpressionSemantics(node);

            if (_scope.GetEnclosingFunction(ignoreDefer: true) is LambdaScope lambda)
                lambda.ThisExpressions.Add(sema);
            else
                Error(
                    node.Span,
                    StandardDiagnosticCodes.MissingEnclosingLambda,
                    "No enclosing lambda expression for 'this' expression");

            return sema;
        }

        public override MetaExpressionSemantics VisitMetaExpression(MetaExpressionSyntax node)
        {
            var oper = VisitExpression(node.Operand);

            return new(node, oper);
        }

        public override AssertExpressionSemantics VisitAssertExpression(AssertExpressionSyntax node)
        {
            var cond = VisitExpression(node.Condition);

            return new(node, cond);
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
                            node.LeftOperand.Span,
                            StandardDiagnosticCodes.ImmutableAssignmentTarget,
                            $"Assignment to immutable symbol '{ident.Syntax.NameToken.Text}'",
                            sym.GetSpans().Select(static loc => (loc, "Symbol defined here")));

                    break;
                case FieldExpressionSemantics or IndexExpressionSemantics:
                    break;
                default:
                    Error(
                        node.LeftOperand.Span,
                        StandardDiagnosticCodes.InvalidAssignmentTarget,
                        "Assignment target must be an identifier, field, or index expression");
                    break;
            }

            var sema = new AssignmentExpressionSemantics(node, left, right);

            sym?.AddAssignment(sema);

            return sema;
        }

        public override FieldExpressionSemantics VisitFieldExpression(FieldExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);

            return new(node, subject);
        }

        public override IndexExpressionSemantics VisitIndexExpression(IndexExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(node.ArgumentList.Arguments, static (@this, arg) => @this.VisitExpression(arg));

            return new(node, subject, args);
        }

        public override CallExpressionSemantics VisitCallExpression(CallExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(node.ArgumentList.Arguments, static (@this, arg) => @this.VisitExpression(arg));
            var defers = node.QuestionToken != null ? _scope.CollectDefers(target: null) : [];

            var sema = new CallExpressionSemantics(node, subject, args, defers);

            if (sema.IsPropagating)
            {
                if (_scope.GetEnclosingTry() is { } @try)
                    @try.CallExpressions.Add(sema);
                else if (_scope.GetEnclosingFunction(ignoreDefer: true) is { IsFallible: true } function)
                    function.CallExpressions.Add(sema);
                else
                    Error(
                        node.Span,
                        StandardDiagnosticCodes.ErrorInInfallibleContext,
                        "Error-propagating call expression in infallible context is invalid");
            }

            return sema;
        }

        public override SendExpressionSemantics VisitSendExpression(SendExpressionSyntax node)
        {
            var subject = VisitExpression(node.Subject);
            var args = ConvertList(node.ArgumentList.Arguments, static (@this, arg) => @this.VisitExpression(arg));

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
                if (!_scope.DefineSymbol<VariableSymbol>(name.Text, out var sym2))
                    _ = _duplicates.Add(sym2);

                sym = Unsafe.As<VariableSymbol>(sym2); // Only variables will be defined at this stage.
            }

            var sema = new VariableBindingSemantics(node, sym);

            sym?.AddBinding(sema);

            return sema;
        }

        public override DiscardBindingSemantics VisitDiscardBinding(DiscardBindingSyntax node)
        {
            if (!_scope.DefineSymbol<VariableSymbol>(node.NameToken.Text, out var sym))
                _ = _duplicates.Add(sym);

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

        public override AggregatePatternFieldSemantics VisitAggregatePatternField(AggregatePatternFieldSyntax node)
        {
            var pat = VisitPattern(node.Pattern);

            return new(node, pat);
        }

        public override ModulePatternSemantics VisitModulePattern(ModulePatternSyntax node)
        {
            var (use, path) = node.Path is { } p ? ResolveModulePath(p) : (null, null);
            var fields = ConvertList(node.Fields, static (@this, field) => @this.VisitAggregatePatternField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Module", "matched");

            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, use, path!, fields, alias);
        }

        public override RecordPatternSemantics VisitRecordPattern(RecordPatternSyntax node)
        {
            var fields = ConvertList(node.Fields, static (@this, field) => @this.VisitAggregatePatternField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Record", "matched");

            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, fields, alias);
        }

        public override ErrorPatternSemantics VisitErrorPattern(ErrorPatternSyntax node)
        {
            var fields = ConvertList(node.Fields, static (@this, field) => @this.VisitAggregatePatternField(field));

            CheckDuplicateFields(fields, static field => field.Syntax.NameToken, "Error", "matched");

            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, fields, alias);
        }

        public override TuplePatternSemantics VisitTuplePattern(TuplePatternSyntax node)
        {
            var comps = ConvertList(node.Components, static (@this, comp) => @this.VisitPattern(comp));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, comps, alias);
        }

        public override ArrayPatternSemantics VisitArrayPattern(ArrayPatternSyntax node)
        {
            var elems = ConvertList(node.Elements, static (@this, elem) => @this.VisitPattern(elem));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, elems, alias);
        }

        public override MapPatternSemantics VisitMapPattern(MapPatternSyntax node)
        {
            var pairs = ConvertList(node.Pairs, static (@this, pair) => @this.VisitMapPatternPair(pair));
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
            var elems = ConvertList(node.Elements, static (@this, elem) => @this.VisitExpression(elem));
            var alias = node.Alias is { } a ? VisitVariableBinding(a.Binding) : null;

            return new(node, elems, alias);
        }
    }

    private readonly AnalysisVisitor _visitor;

    public LanguageAnalyzer(
        SyntaxTree tree, InteractiveContext context, ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        _visitor = new(tree, context, diagnostics);
    }

    public DocumentSemantics Analyze()
    {
        return _visitor.Analyze();
    }
}
