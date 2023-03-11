using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UndocumentedPublicDeclarationPass : LintPass
{
    public static UndocumentedPublicDeclarationPass Instance { get; } = new();

    private UndocumentedPublicDeclarationPass()
        : base(
            "undocumented-public-declaration",
            DiagnosticSeverity.Warning,
            LintTargets.Document | LintTargets.Declaration,
            SyntaxMode.Module)
    {
    }

    protected internal override void Run(LintContext context, DocumentSemantics document)
    {
        var module = Unsafe.As<ModuleDocumentSemantics>(document);
        var syntax = module.Syntax;

        CheckDocumentationAttribute(
            context, "Module", syntax.ModKeywordToken, module.Attributes.Select(static attr => attr.Name), null);

        // Types are not part of the semantic tree, but we still want to check public types.
        foreach (var decl in syntax.Declarations)
            if (decl is TypeDeclarationSyntax { PubKeywordToken: { }, NameToken: { IsMissing: false } name })
                CheckDocumentationAttribute(
                    context,
                    "Public type",
                    name,
                    decl.Attributes.Select(static attr => attr.NameToken.Text),
                    name.Text);
    }

    protected internal override void Run(LintContext context, DeclarationSemantics declaration)
    {
        // If the module is explicitly undocumented, do not require declarations within it to be decorated.
        if (Unsafe.As<ModuleDocumentSemantics>(declaration.Parent).Attributes.Any(
            attr => attr is { Name: "doc", Value: false }))
            return;

        var (kind, pub, name) = declaration switch
        {
            ConstantDeclarationSemantics constant => ("Public constant", constant.IsPublic, constant.Syntax.NameToken),
            FunctionDeclarationSemantics function => ("Public function", function.IsPublic, function.Syntax.NameToken),
            _ => (null, false, null),
        };

        if (pub && !name!.IsMissing)
            CheckDocumentationAttribute(
                context, kind!, name, declaration.Attributes.Select(static attr => attr.Name), name.Text);
    }

    private static void CheckDocumentationAttribute(
        LintContext context, string kind, SyntaxItem item, IEnumerable<string> attributes, string? name)
    {
        if (!attributes.Any(static t => t == "doc"))
            context.ReportDiagnostic(
                item.Span,
                $"{kind}{(name != null ? $" '{name}'" : string.Empty)} should be decorated with a 'doc' attribute");
    }
}
