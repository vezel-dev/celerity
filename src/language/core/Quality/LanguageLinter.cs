using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality;

internal sealed partial class LanguageLinter
{
    private readonly SemanticTree _tree;

    private readonly IEnumerable<LintPass> _passes;

    private readonly LintConfiguration _configuration;

    private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

    public LanguageLinter(
        SemanticTree tree,
        IEnumerable<LintPass> passes,
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
        var mode = _tree.Root is ModuleDocumentSemantics ? SyntaxMode.Module : SyntaxMode.Interactive;

        foreach (var pass in _passes)
            if (pass.Mode == null || pass.Mode == mode)
                pass.Run(new(_tree, pass, _diagnostics));

        var cfgs = new Stack<LintConfiguration>(1);

        void AdjustSeverities(SemanticNode node)
        {
            bool PushConfiguration(SemanticNodeList<AttributeSemantics, AttributeSyntax> attributes)
            {
                var current = cfgs.Peek();
                var replacement = current;

                foreach (var attr in attributes)
                {
                    if (attr is not { Name: "lint", Value: ReadOnlyMemory<byte> utf8 } ||
                        Encoding.UTF8.GetString(utf8.Span).Split(':', StringSplitOptions.RemoveEmptyEntries) is
                            not [var left, var right] ||
                        !DiagnosticCode.TryCreate(left, out var code))
                        continue;

                    var severity = right switch
                    {
                        "none" => DiagnosticSeverity.None,
                        "warning" => DiagnosticSeverity.Warning,
                        "error" => DiagnosticSeverity.Error,
                        _ => default(DiagnosticSeverity?),
                    };

                    if (severity is { } sev)
                        replacement = current.WithSeverity(code, sev);
                }

                if (replacement != current)
                {
                    cfgs.Push(replacement);

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

            var span = node.Syntax.Span;
            var cfg = cfgs.Peek();

            for (var i = 0; i < _diagnostics.Count; i++)
            {
                var diag = _diagnostics[i];

                if (span.Contains(diag.Span.Start) &&
                    cfg.TryGetSeverity(diag.Code, out var severity) &&
                    severity != diag.Severity)
                    _diagnostics[i] = new(diag.Tree, diag.Span, diag.Code, severity, diag.Message, diag.Notes);
            }

            if (node.HasChildren)
                foreach (var child in node.Children())
                    AdjustSeverities(child);

            if (pushed)
                _ = cfgs.Pop();
        }

        cfgs.Push(_configuration);

        AdjustSeverities(_tree.Root);

        _ = cfgs.Pop();
    }
}
