// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeModule : RuntimeMetadata
{
    public ModulePath Path { get; }

    public ImmutableDictionary<string, RuntimeConstant> Constants { get; }

    public ImmutableDictionary<string, RuntimeFunction> Functions { get; }

    public ImmutableDictionary<string, RuntimeTest> Tests { get; }

    private int _lambdaId;

    internal RuntimeModule(
        ModulePath path,
        ModuleDocumentSemantics module,
        IEnumerable<CodeDeclarationSemantics> declarations,
        IEnumerable<LambdaExpressionSemantics> lambdas)
        : base(module.Attributes)
    {
        Path = path;

        var constants = new List<RuntimeConstant>();
        var functions = new List<RuntimeFunction>();
        var tests = new List<RuntimeTest>();

        foreach (var decl in declarations)
        {
            switch (decl)
            {
                case ConstantDeclarationSemantics constant:
                    constants.Add(new(this, constant));
                    break;
                case FunctionDeclarationSemantics function:
                    functions.Add(new(this, function));
                    break;
                case TestDeclarationSemantics test:
                    tests.Add(new(this, test));
                    break;
            }
        }

        foreach (var lambda in lambdas)
            functions.Add(new(this, lambda));

        Constants = constants.ToImmutableDictionary(static constant => constant.Name, static constant => constant);
        Functions = functions.ToImmutableDictionary(static function => function.Name, static function => function);
        Tests = tests.ToImmutableDictionary(static test => test.Name, static test => test);
    }

    internal int AllocateLambdaId()
    {
        return _lambdaId++;
    }

    public override string ToString()
    {
        return $"mod {Path}";
    }
}
