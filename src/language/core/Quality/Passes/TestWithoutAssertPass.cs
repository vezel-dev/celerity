using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class TestWithoutAssertPass : LintPass
{
    public static TestWithoutAssertPass Instance { get; } = new();

    private TestWithoutAssertPass()
        : base("test-without-assert")
    {
    }

    protected internal override void Run(LintPassContext context)
    {
        foreach (var decl in Unsafe.As<ModuleDocumentSemantics>(context.Root).Declarations)
            if (decl is TestDeclarationSemantics { Symbol: { } sym } test &&
                test.Body.Descendants().All(static node => node is not AssertStatementSemantics))
                context.ReportDiagnostic(
                    test.Syntax.NameToken.Span, $"Test declaration '{sym.Name}' lacks 'assert' statements");
    }
}
