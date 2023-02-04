namespace Vezel.Celerity.Generators.Ast;

[Generator]
public sealed class AstGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.AdditionalTextsProvider
                .Where(static at => Path.GetFileName(at.Path) == "Ast.xml"),
            (ctx, file) =>
            {
                var text = file.GetText(ctx.CancellationToken)!.ToString();
                var settings = new XmlReaderSettings
                {
                    XmlResolver = new XmlUrlResolver(),
                    ValidationType = ValidationType.Schema,
                    ValidationFlags =
                        XmlSchemaValidationFlags.ProcessSchemaLocation |
                        XmlSchemaValidationFlags.ReportValidationWarnings,
                };

                using var stringReader = new StringReader(file.GetText(ctx.CancellationToken)!.ToString());
                using var xmlReader = XmlReader.Create(stringReader, settings);

                var tree = (AstTree)new XmlSerializer(typeof(AstTree)).Deserialize(xmlReader);
                var baseName = Path.GetFileName(file.Path);

                foreach (var type in tree.Types ?? Array.Empty<AstType>())
                    GenerateType(
                        ctx, Path.ChangeExtension(baseName, $"{type.Name}{tree.Settings.NameSuffix}.g.cs"), tree, type);
            });
    }

    private static void GenerateType(SourceProductionContext context, string name, AstTree tree, AstType type)
    {
        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        var settings = tree.Settings;

        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine($"namespace {settings.Namespace};");
        writer.WriteLine();

        var typeName = tree.GetTypeName(type.Name);
        var baseTypeName = type.Base is string b ? tree.GetTypeName(b) : tree.GetTypeName(settings.BaseType);

        writer.WriteLine($"public {(type.Abstract ? "abstract" : "sealed")} class {typeName} : {baseTypeName}");
        writer.WriteLine("{");

        writer.Indent++;

        if (!type.Root)
        {
            writer.WriteLine($"public new SyntaxNode Parent => base.Parent!;");
            writer.WriteLine();
        }

        var props = type.Properties ?? Array.Empty<AstProperty>();

        foreach (var prop in props)
        {
            var mod = string.Empty;

            if (type.Abstract)
                mod = " abstract";
            else if (prop.Override)
                mod = " override";

            writer.WriteLine($"public{mod} {prop.GetTypeName(tree)} {prop.GetPropertyName()} {{ get; }}");
            writer.WriteLine();
        }

        if (type.Abstract)
        {
            writer.WriteLine($"private protected {typeName}()");
            writer.WriteLine("{");
            writer.WriteLine("}");
        }
        else
        {
            var nodes = props.Where(p => p is AstNodeProperty or AstNodesProperty).ToArray();

            writer.Write("public override bool HasNodes");

            if (nodes.Length == 0)
                writer.WriteLine(" => false;");
            else if (nodes.Any(p => p is AstNodeProperty { Optional: false }))
                writer.WriteLine(" => true;");
            else
            {
                writer.WriteLine();
                writer.WriteLine("{");

                writer.Indent++;

                writer.WriteLine("get");
                writer.WriteLine("{");

                writer.Indent++;

                for (var i = 0; i < nodes.Length; i++)
                {
                    var prop = nodes[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case AstNodeProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case AstNodesProperty { Separated: true }:
                            writer.WriteLine($"if ({propName}.Items.Count != 0)");
                            break;
                        default:
                            writer.WriteLine($"if ({propName}.Count != 0)");
                            break;
                    }

                    writer.Indent++;

                    writer.WriteLine("return true;");

                    writer.Indent--;

                    if (i != nodes.Length - 1)
                        writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine("return false;");

                writer.Indent--;

                writer.WriteLine("}");

                writer.Indent--;

                writer.WriteLine("}");
            }

            writer.WriteLine();

            var tokens = props.Where(
                p => p is AstTokenProperty or AstTokensProperty or AstNodesProperty { Separated: true }).ToArray();

            writer.Write("public override bool HasTokens");

            if (tokens.Length == 0)
                writer.WriteLine(" => false;");
            else if (tokens.Any(p => p is AstTokenProperty { Optional: false }))
                writer.WriteLine(" => true;");
            else
            {
                writer.WriteLine();
                writer.WriteLine("{");

                writer.Indent++;

                writer.WriteLine("get");
                writer.WriteLine("{");

                writer.Indent++;

                for (var i = 0; i < tokens.Length; i++)
                {
                    var prop = tokens[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case AstTokenProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case AstTokensProperty:
                            writer.WriteLine($"if ({propName}.Count != 0)");
                            break;
                        default:
                            writer.WriteLine($"if ({propName}.Separators.Count != 0)");
                            break;
                    }

                    writer.Indent++;

                    writer.WriteLine("return true;");

                    writer.Indent--;

                    if (i != tokens.Length - 1)
                        writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine("return false;");

                writer.Indent--;

                writer.WriteLine("}");

                writer.Indent--;

                writer.WriteLine("}");
            }

            writer.WriteLine();

            if (props.Length != 0)
            {
                writer.WriteLine($"internal {typeName}(");

                writer.Indent++;

                for (var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];

                    writer.Write("{0} {1}", prop.GetTypeName(tree), prop.GetParameterName());

                    if (i == props.Length - 1)
                        writer.Write(")");
                    else
                        writer.Write(",");

                    writer.WriteLine();
                }

                writer.Indent--;

                writer.WriteLine("{");

                writer.Indent++;

                foreach (var prop in props)
                    writer.WriteLine($"{prop.GetPropertyName()} = {prop.GetParameterName()};");

                foreach (var prop in props)
                {
                    writer.WriteLine();

                    var param = prop.GetParameterName();

                    switch (prop)
                    {
                        case AstTokenProperty { Optional: true } or AstNodeProperty { Optional: true }:
                            writer.WriteLine($"{param}?.SetParent(this);");
                            break;
                        case AstTokenProperty or AstNodeProperty:
                            writer.WriteLine($"{param}.SetParent(this);");
                            break;
                        default:
                            writer.WriteLine($"foreach (var item in {param})");

                            writer.Indent++;

                            writer.WriteLine("item.SetParent(this);");

                            writer.Indent--;

                            break;
                    }
                }

                writer.Indent--;

                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine($"internal {typeName}()");
                writer.WriteLine("{");
                writer.WriteLine("}");
            }

            writer.WriteLine();

            writer.WriteLine("public override IEnumerable<SyntaxItem> Children()");
            writer.WriteLine("{");

            writer.Indent++;

            if (props.Length != 0)
            {
                for (var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case AstTokenProperty { Optional: true } or AstNodeProperty { Optional: true }:
                            writer.WriteLine($"if ({propName} != null)");

                            writer.Indent++;

                            writer.WriteLine($"yield return {propName};");

                            writer.Indent--;
                            break;
                        case AstTokenProperty or AstNodeProperty:
                            writer.WriteLine($"yield return {propName};");
                            break;
                        default:
                            writer.WriteLine($"foreach (var item in {propName})");

                            writer.Indent++;

                            writer.WriteLine("yield return item;");

                            writer.Indent--;
                            break;
                    }

                    if (i != props.Length - 1)
                        writer.WriteLine();
                }
            }
            else
                writer.WriteLine("return Array.Empty<SyntaxItem>();");

            writer.Indent--;

            writer.WriteLine("}");

            writer.WriteLine();

            writer.WriteLine("internal override T Visit<T>(SyntaxWalker<T> walker, T state)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("return walker.Visit(this, state);");

            writer.Indent--;

            writer.WriteLine("}");
        }

        writer.Indent--;

        writer.WriteLine("}");

        writer.WriteLine();

        writer.WriteLine("public abstract partial class SyntaxWalker<T>");
        writer.WriteLine("{");

        writer.Indent++;

        writer.WriteLine($"public virtual T Visit({tree.GetTypeName(type.Name)} node, T state)");
        writer.WriteLine("{");

        writer.Indent++;

        writer.WriteLine("Check.Null(node);");
        writer.WriteLine();
        writer.WriteLine($"return DefaultVisitNode(node, state);");

        writer.Indent--;

        writer.WriteLine("}");

        writer.Indent--;

        writer.WriteLine("}");

        context.AddSource(name, sb.ToString());
    }
}
