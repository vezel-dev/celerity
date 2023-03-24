using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class TestWithoutAssertPass : LintPass
{
    public static TestWithoutAssertPass Instance { get; } = new();

    private TestWithoutAssertPass()
        : base("test-without-assert", DiagnosticSeverity.Warning, SyntaxMode.Module)
    {
    }

    protected internal override void Run(LintContext context)
    {
        foreach (var decl in Unsafe.As<ModuleDocumentSemantics>(context.Tree.Root).Declarations)
            if (decl is TestDeclarationSemantics { Symbol: { } sym } test &&
                test.Body.Descendants().All(static node => node is not AssertStatementSemantics))
                context.ReportDiagnostic(
                    test.Syntax.NameToken.Span, $"Test declaration '{sym.Name}' lacks 'assert' statements");
    }
}
