namespace Vezel.Celerity.Quality;

internal sealed partial class LanguageLinter
{
    private sealed partial class LintWalker : SemanticWalker<object?>
    {
        private readonly Stack<LintConfiguration> _configurations = new(1);

        private readonly ReadOnlyMemory<LintPass> _passes;

        private readonly LintConfiguration _configuration;

        private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

        public LintWalker(
            ReadOnlyMemory<LintPass> passes,
            LintConfiguration configuration,
            ImmutableArray<SourceDiagnostic>.Builder diagnostics)
        {
            _passes = passes;
            _configuration = configuration;
            _diagnostics = diagnostics;
        }

        public void Lint(DocumentSemantics document)
        {
            _configurations.Push(_configuration);

            _ = VisitNode(document, null);

            _ = _configurations.Pop();
        }

        protected override object? DefaultVisitNode(SemanticNode node, object? state)
        {
            bool PushConfiguration(SemanticNodeList<AttributeSemantics> attributes)
            {
                var current = _configurations.Peek();
                var replacement = current;

                foreach (var attr in attributes)
                {
                    if (attr is not { Name: "lint", Value: ReadOnlyMemory<byte> utf8 } ||
                        SeverityRegex().Match(Encoding.UTF8.GetString(utf8.Span)) is not { Success: true } match ||
                        !SourceDiagnosticCode.TryCreate(match.Groups[1].Value, out var code))
                        continue;

                    var (severity, valid) = match.Groups[2].ValueSpan switch
                    {
                        var name when Enum.TryParse<SourceDiagnosticSeverity>(name, out var sev) => (sev, true),
                        "none" => (null, true),
                        _ => (default(SourceDiagnosticSeverity?), false),
                    };

                    if (valid)
                        current = current.WithSeverity(code, severity);
                }

                if (replacement != current)
                {
                    _configurations.Push(replacement);

                    return true;
                }

                return false;
            }

            var pushed = node switch
            {
                ModuleDocumentSemantics module => PushConfiguration(module.Attributes),
                DeclarationSemantics declaration => PushConfiguration(declaration.Attributes),
                StatementSemantics statement => PushConfiguration(statement.Attributes),
                _ => false,
            };

            void RunLints<T>(LintTargets target, T node, Action<LintPass, LintContext, T> runner)
                where T : SemanticNode
            {
                var cfg = _configurations.Peek();

                foreach (var pass in _passes.Span)
                {
                    if ((pass.Targets & target) != target)
                        continue;

                    if (!cfg.TryGetSeverity(pass.Code, out var severity))
                        severity = pass.Severity;

                    if (severity is not { } sev)
                        continue;

                    runner(pass, new(pass.Code, sev, _diagnostics), node);
                }
            }

            switch (node)
            {
                case DocumentSemantics module:
                    RunLints(LintTargets.Document, module, static (pass, ctx, node) => pass.Run(ctx, node));
                    break;
                case DeclarationSemantics declaration:
                    RunLints(LintTargets.Declaration, declaration, static (pass, ctx, node) => pass.Run(ctx, node));
                    break;
                case StatementSemantics statement:
                    RunLints(LintTargets.Statement, statement, static (pass, ctx, node) => pass.Run(ctx, node));
                    break;
                case ExpressionSemantics expression:
                    RunLints(LintTargets.Expression, expression, static (pass, ctx, node) => pass.Run(ctx, node));
                    break;
                case PatternSemantics pattern:
                    RunLints(LintTargets.Pattern, pattern, static (pass, ctx, node) => pass.Run(ctx, node));
                    break;
            }

            state = base.DefaultVisitNode(node, state);

            if (pushed)
                _ = _configurations.Pop();

            return state;
        }

        [GeneratedRegex(@"^(.*):(.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
        private static partial Regex SeverityRegex();
    }

    private readonly DocumentSemantics _document;

    private readonly LintWalker _walker;

    public LanguageLinter(
        DocumentSemantics document,
        IEnumerable<LintPass> passes,
        LintConfiguration configuration,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _document = document;

        var mode = document is ModuleDocumentSemantics ? SyntaxMode.Module : SyntaxMode.Interactive;

        _walker = new(
            passes.Where(pass => pass.Mode == null || pass.Mode == mode).ToArray(), configuration, diagnostics);
    }

    public void Lint()
    {
        _walker.Lint(_document);
    }
}
