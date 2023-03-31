using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality;

public sealed class LintDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private readonly ReadOnlyMemory<LintPass> _passes;

    private readonly LintConfiguration _configuration;

    public LintDiagnosticAnalyzer(IEnumerable<LintPass> passes, LintConfiguration configuration)
    {
        Check.Null(passes);
        Check.All(passes, static pass => pass != null);
        Check.Null(configuration);

        _passes = passes.ToArray();
        _configuration = configuration;
    }

    protected internal override void Analyze(DiagnosticAnalyzerContext context)
    {
        Check.Null(context);
        Check.Argument(context.Root is ModuleDocumentSemantics, context);

        var root = context.Root;
        var diags = new List<Diagnostic>();

        foreach (var pass in _passes.Span)
        {
            var ctx = new LintPassContext(root, pass, diags);

            pass.Run(ctx);
            ctx.Invalidate();
        }

        var cfgs = new Stack<LintConfiguration>();

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
                CodeParameterSemantics parameter => PushConfiguration(parameter.Attributes),
                DeclarationSemantics declaration => PushConfiguration(declaration.Attributes),
                StatementSemantics statement => PushConfiguration(statement.Attributes),
                _ => false,
            };

            var span = node.Syntax.Span;
            var cfg = cfgs.Peek();

            for (var i = 0; i < diags.Count; i++)
            {
                var diag = diags[i];

                if (span.Contains(diag.Span.Start) &&
                    cfg.TryGetSeverity(diag.Code, out var severity) &&
                    severity != diag.Severity)
                    diags[i] = Diagnostic.Create(
                        diag.Tree,
                        diag.Span,
                        severity,
                        diag.Code,
                        diag.Message,
                        diag.Notes.Select(static note => (note.Span, note.Message)));
            }

            if (node.HasChildren)
                foreach (var child in node.Children())
                    AdjustSeverities(child);

            if (pushed)
                _ = cfgs.Pop();
        }

        cfgs.Push(_configuration);

        AdjustSeverities(root);

        _ = cfgs.Pop();

        foreach (var diag in diags)
            context.AddDiagnostic(diag);
    }
}
