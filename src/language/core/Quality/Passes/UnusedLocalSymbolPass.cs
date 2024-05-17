// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Binding;
using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UnusedLocalSymbolPass : LintPass
{
    public static UnusedLocalSymbolPass Instance { get; } = new();

    private UnusedLocalSymbolPass()
        : base("unused-local-symbol")
    {
    }

    protected internal override void Run(LintPassContext context)
    {
        var syms = new HashSet<Symbol>();

        foreach (var node in context.Root.Descendants())
        {
            var sym = node switch
            {
                CodeParameterSemantics parameter => parameter.Symbol,
                ConstantDeclarationSemantics { IsPublic: false } constant => constant.Symbol,
                FunctionDeclarationSemantics { IsPublic: false } function => function.Symbol,
                VariableBindingSemantics variable => variable.Symbol,
                _ => default(Symbol),
            };

            // If a symbol is bound to a test declaration, we are not interested in it. This can happen if the module
            // erroneously has a test declaration with the same name as e.g. a function declaration.
            if (sym is { References.Length: 0 } &&
                sym.Bindings.All(static node => node is not TestDeclarationSemantics))
                _ = syms.Add(sym);
        }

        foreach (var sym in syms)
        {
            var kind = sym switch
            {
                DeclarationSymbol => "Declaration",
                ParameterSymbol => "Parameter",
                VariableSymbol => "Variable",
                _ => throw new UnreachableException(),
            };

            foreach (var span in sym.GetSpans())
                context.ReportDiagnostic(span, $"{kind} '{sym.Name}' is unused");
        }
    }
}
