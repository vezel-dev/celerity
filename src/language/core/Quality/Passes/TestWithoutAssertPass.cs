// SPDX-License-Identifier: 0BSD

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
            if (decl is TestDeclarationSemantics { Syntax.NameToken: { IsMissing: false } name } test &&
                test.Body.Descendants().All(static node => node is not AssertExpressionSemantics))
                context.ReportDiagnostic(
                    name.Span, $"Test declaration '{name.Text}' lacks 'assert' expressions");
    }
}
