using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality;

internal sealed partial class LanguageLinter
{
    private sealed partial class LintVisitor : SemanticVisitor
    {
        private readonly SemanticTree _tree;

        private readonly ReadOnlyMemory<LintPass> _passes;

        private readonly LintConfiguration _configuration;

        private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

        private readonly Stack<LintConfiguration> _configurations = new(1);

        public LintVisitor(
            SemanticTree tree,
            ReadOnlyMemory<LintPass> passes,
            LintConfiguration configuration,
            ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            _tree = tree;
            _passes = passes;
            _configuration = configuration;
            _diagnostics = diagnostics;
        }

        public void Lint()
        {
            _configurations.Push(_configuration);

            Visit(_tree.Root);

            _ = _configurations.Pop();
        }

        protected override void DefaultVisit(SemanticNode node)
        {
            bool PushConfiguration(SemanticNodeList<AttributeSemantics, AttributeSyntax> attributes)
            {
                var current = _configurations.Peek();
                var replacement = current;

                foreach (var attr in attributes)
                {
                    if (attr is not { Name: "lint", Value: ReadOnlyMemory<byte> utf8 } ||
                        Encoding.UTF8.GetString(utf8.Span).Split(':', StringSplitOptions.RemoveEmptyEntries) is
                            not [var left, var right] ||
                        !DiagnosticCode.TryCreate(left, out var code))
                        continue;

                    var (severity, valid) = right switch
                    {
                        "none" => (null, true),
                        "warning" => (DiagnosticSeverity.Warning, true),
                        "error" => (DiagnosticSeverity.Error, true),
                        _ => (default(DiagnosticSeverity?), false),
                    };

                    if (valid)
                        replacement = current.WithSeverity(code, severity);
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

                    runner(pass, new(_tree.Syntax, pass.Code, sev, _diagnostics), node);
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

            if (node.HasChildren)
                foreach (var child in node.Children())
                    Visit(child);

            if (pushed)
                _ = _configurations.Pop();
        }
    }

    private readonly LintVisitor _visitor;

    public LanguageLinter(
        SemanticTree tree,
        IEnumerable<LintPass> passes,
        LintConfiguration configuration,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        var mode = tree.Root is ModuleDocumentSemantics ? SyntaxMode.Module : SyntaxMode.Interactive;

        _visitor = new(
            tree, passes.Where(pass => pass.Mode == null || pass.Mode == mode).ToArray(), configuration, diagnostics);
    }

    public void Lint()
    {
        _visitor.Lint();
    }
}
