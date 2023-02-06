namespace Vezel.Celerity.Semantics;

internal sealed class SemanticTables
{
    public ImmutableDictionary<DeclarationNode, Declaration> Declarations { get; }

    public ImmutableDictionary<LambdaExpressionNode, LambdaFunction> Lambdas { get; }

    public ImmutableDictionary<SyntaxNode, Scope> Scopes { get; }

    public ImmutableDictionary<SyntaxNode, Symbol> Symbols { get; }

    public SemanticTables(SemanticTablesBuilder builder)
    {
        Declarations = builder.Declarations.ToImmutable();
        Lambdas = builder.Lambdas.ToImmutable();
        Scopes = builder.Scopes.ToImmutable();
        Symbols = builder.Symbols.ToImmutable();
    }
}
