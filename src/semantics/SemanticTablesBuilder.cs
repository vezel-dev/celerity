namespace Vezel.Celerity.Semantics;

internal sealed class SemanticTablesBuilder
{
    public ImmutableDictionary<DeclarationNode, Declaration>.Builder Declarations { get; } =
        ImmutableDictionary.CreateBuilder<DeclarationNode, Declaration>();

    public ImmutableDictionary<LambdaExpressionNode, LambdaFunction>.Builder Lambdas { get; } =
        ImmutableDictionary.CreateBuilder<LambdaExpressionNode, LambdaFunction>();

    public ImmutableDictionary<SyntaxNode, Scope>.Builder Scopes { get; } =
        ImmutableDictionary.CreateBuilder<SyntaxNode, Scope>();

    public ImmutableDictionary<SyntaxNode, Symbol>.Builder Symbols { get; } =
        ImmutableDictionary.CreateBuilder<SyntaxNode, Symbol>();
}
