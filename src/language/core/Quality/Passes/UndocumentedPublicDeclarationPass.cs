using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UndocumentedPublicDeclarationPass : LintPass
{
    public static UndocumentedPublicDeclarationPass Instance { get; } = new();

    private UndocumentedPublicDeclarationPass()
        : base("undocumented-public-declaration")
    {
    }

    protected internal override void Run(LintPassContext context)
    {
        var module = Unsafe.As<ModuleDocumentSemantics>(context.Root);
        var syntax = module.Syntax;

        void CheckDocumentationAttribute(
            string kind, SyntaxItem item, IEnumerable<AttributeSyntax> attributes, string? name)
        {
            if (!attributes.Any(static attr => attr.NameToken.Text == "doc"))
                context.ReportDiagnostic(
                    item.Span,
                    $"{kind}{(name != null ? $" '{name}'" : string.Empty)} should be decorated with a 'doc' attribute");
        }

        CheckDocumentationAttribute("Module", syntax.ModKeywordToken, syntax.Attributes, null);

        // If the module is explicitly undocumented, do not require declarations within it to be decorated.
        if (module.Attributes.Any(static attr => attr is { Name: "doc", Value: false }))
            return;

        foreach (var decl in syntax.Declarations)
        {
            // Types are not part of the semantic tree, but we still want to check public types.
            var tup = decl switch
            {
                TypeDeclarationSyntax type => ("Public type", type.PubKeywordToken, type.NameToken),
                ConstantDeclarationSyntax constant => ("Public constant", constant.PubKeywordToken, constant.NameToken),
                FunctionDeclarationSyntax function => ("Public function", function.PubKeywordToken, function.NameToken),
                _ => default((string, SyntaxToken?, SyntaxToken)?),
            };

            if (tup is (var kind, { }, var name))
                CheckDocumentationAttribute(kind, name, decl.Attributes, name.Text);
        }
    }
}
