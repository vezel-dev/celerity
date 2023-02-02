namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeModule : RuntimeMetadata
{
    public ModulePath Path { get; }

    public ImmutableDictionary<string, RuntimeConstant> Constants { get; }

    public ImmutableDictionary<string, RuntimeFunction> Functions { get; }

    public ImmutableDictionary<string, RuntimeTest> Tests { get; }

    internal RuntimeModule(
        Semantics.Module module, IEnumerable<CodeDeclaration> declarations, IEnumerable<LambdaFunction> lambdas)
        : base(module.Attributes)
    {
        Path = module.Path;

        var constants = new List<RuntimeConstant>();
        var functions = new List<RuntimeFunction>();
        var tests = new List<RuntimeTest>();

        foreach (var decl in declarations)
        {
            switch (decl)
            {
                case ConstantDeclaration constant:
                    constants.Add(new(this, constant));
                    break;
                case FunctionDeclaration function:
                    functions.Add(new(this, function));
                    break;
                case TestDeclaration test:
                    tests.Add(new(this, test));
                    break;
            }
        }

        foreach (var lambda in lambdas)
            functions.Add(new(this, lambda));

        Constants = constants.ToImmutableDictionary(constant => constant.Name, constant => constant);
        Functions = functions.ToImmutableDictionary(function => function.Name, function => function);
        Tests = tests.ToImmutableDictionary(test => test.Name, test => test);
    }
}
