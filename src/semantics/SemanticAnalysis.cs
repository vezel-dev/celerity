namespace Vezel.Celerity.Semantics;

public sealed class SemanticAnalysis
{
    public SyntaxTree Tree { get; }

    public Module? Module { get; }

    public ImmutableArray<SourceDiagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diag => diag.IsError);

    private readonly ImmutableDictionary<DeclarationNode, Declaration> _declarations;

    private readonly ImmutableDictionary<LambdaExpressionNode, LambdaFunction> _lambdas;

    private readonly ImmutableDictionary<SyntaxNode, Scope> _scopes;

    private readonly ImmutableDictionary<SyntaxNode, Symbol> _symbols;

    private SemanticAnalysis(
        SyntaxTree tree, Module? module, SemanticTables tables, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        Tree = tree;
        Module = module;
        Diagnostics = diagnostics;
        _declarations = tables.Declarations;
        _lambdas = tables.Lambdas;
        _scopes = tables.Scopes;
        _symbols = tables.Symbols;
    }

    public static SemanticAnalysis Analyze(SyntaxTree tree)
    {
        Check.Null(tree);

        var root = tree.Root;
        var scope = new RootScope(root);
        var builder = new SemanticTablesBuilder();
        var diags = ImmutableArray.CreateBuilder<SourceDiagnostic>();

        builder.Scopes.Add(root, scope);
        diags.AddRange(tree.Diagnostics);

        _ = new LanguageAnalyzer(scope, builder, diags).VisitNode(root, null);

        return new(tree, root is ModuleNode mod ? new(mod) : null, new(builder), diags.ToImmutable());
    }

    [SuppressMessage("", "CA1024")]
    public IEnumerable<Declaration> GetDeclarations()
    {
        return _declarations.Values;
    }

    public IEnumerable<UseDeclaration> GetUseDeclarations()
    {
        return _declarations.Values.OfType<UseDeclaration>();
    }

    public IEnumerable<TypeDeclaration> GetTypeDeclarations()
    {
        return _declarations.Values.OfType<TypeDeclaration>();
    }

    public IEnumerable<CodeDeclaration> GetCodeDeclarations()
    {
        return _declarations.Values.OfType<CodeDeclaration>();
    }

    public IEnumerable<ConstantDeclaration> GetConstantDeclarations()
    {
        return _declarations.Values.OfType<ConstantDeclaration>();
    }

    public IEnumerable<FunctionDeclaration> GetFunctionDeclarations()
    {
        return _declarations.Values.OfType<FunctionDeclaration>();
    }

    public IEnumerable<TestDeclaration> GetTestDeclarations()
    {
        return _declarations.Values.OfType<TestDeclaration>();
    }

    public UseDeclaration GetDeclaration(UseDeclarationNode declaration)
    {
        return Unsafe.As<UseDeclaration>(GetDeclarationCore(declaration));
    }

    public TypeDeclaration GetDeclaration(TypeDeclarationNode declaration)
    {
        return Unsafe.As<TypeDeclaration>(GetDeclarationCore(declaration));
    }

    public ConstantDeclaration GetDeclaration(ConstantDeclarationNode declaration)
    {
        return Unsafe.As<ConstantDeclaration>(GetDeclarationCore(declaration));
    }

    public FunctionDeclaration GetDeclaration(FunctionDeclarationNode declaration)
    {
        return Unsafe.As<FunctionDeclaration>(GetDeclarationCore(declaration));
    }

    public TestDeclaration GetDeclaration(TestDeclarationNode declaration)
    {
        return Unsafe.As<TestDeclaration>(GetDeclarationCore(declaration));
    }

    private Declaration GetDeclarationCore(DeclarationNode node)
    {
        Check.Null(node);
        Check.Argument(_declarations.TryGetValue(node, out var decl), node);

        return decl;
    }

    public LambdaFunction GetLambda(LambdaExpressionNode expression)
    {
        Check.Null(expression);
        Check.Argument(_lambdas.TryGetValue(expression, out var lambda), expression);

        return lambda;
    }

    public RootScope GetScope(RootNode root)
    {
        return Unsafe.As<RootScope>(GetScopeCore(root));
    }

    public FunctionScope GetScope(FunctionDeclarationNode declaration)
    {
        return Unsafe.As<FunctionScope>(GetScopeCore(declaration));
    }

    public BlockScope GetScope(BlockExpressionNode expression)
    {
        return Unsafe.As<BlockScope>(GetScopeCore(expression));
    }

    public LambdaScope GetScope(LambdaExpressionNode expression)
    {
        return Unsafe.As<LambdaScope>(GetScopeCore(expression));
    }

    public LoopScope GetScope(LoopExpressionNode expression)
    {
        return Unsafe.As<LoopScope>(GetScopeCore(expression));
    }

    public PatternScope GetScope(PatternNode pattern)
    {
        return Unsafe.As<PatternScope>(GetScopeCore(pattern));
    }

    private Scope GetScopeCore(SyntaxNode node)
    {
        Check.Null(node);
        Check.Argument(_scopes.TryGetValue(node, out var scope), node);

        return scope;
    }

    public DeclarationSymbol GetSymbol(CodeDeclarationNode declaration)
    {
        return Unsafe.As<DeclarationSymbol>(GetSymbolCore(declaration));
    }

    public ParameterSymbol GetSymbol(CodeParameterNode parameter)
    {
        return Unsafe.As<ParameterSymbol>(GetSymbolCore(parameter));
    }

    public VariableSymbol GetSymbol(PatternVariableBindingNode binding)
    {
        return Unsafe.As<VariableSymbol>(GetSymbolCore(binding));
    }

    private Symbol GetSymbolCore(SyntaxNode node)
    {
        Check.Null(node);
        Check.Argument(_symbols.TryGetValue(node, out var symbol), node);

        return symbol;
    }
}
