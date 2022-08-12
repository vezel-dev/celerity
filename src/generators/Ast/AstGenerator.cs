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

                foreach (var node in tree.Nodes ?? Array.Empty<AstNode>())
                    GenerateNode(
                        ctx,
                        Path.ChangeExtension(baseName, $"{node.Name}{tree.Settings.NameSuffix}.g.cs"),
                        tree,
                        node);
            });
    }

    private static void GenerateNode(SourceProductionContext context, string name, AstTree tree, AstNode node)
    {
        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        var settings = tree.Settings;

        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine($"namespace {settings.Namespace};");
        writer.WriteLine();

        var typeName = tree.GetNodeTypeName(node.Name);
        var baseTypeName = node.Base is string b ? tree.GetNodeTypeName(b) : tree.GetNodeTypeName(settings.BaseType);

        writer.WriteLine($"public {(node.Abstract ? "abstract" : "sealed")} class {typeName} : {baseTypeName}");
        writer.WriteLine("{");

        writer.Indent++;

        var props = node.Properties ?? Array.Empty<AstProperty>();

        foreach (var prop in props)
        {
            var mod = string.Empty;

            if (node.Abstract)
                mod = " abstract";
            else if (prop.Override)
                mod = " override";

            writer.WriteLine($"public{mod} {prop.GetTypeName(tree)} {prop.GetPropertyName()} {{ get; }}");
            writer.WriteLine();
        }

        if (node.Abstract)
        {
            writer.WriteLine($"private protected {typeName}()");
            writer.WriteLine("{");
            writer.WriteLine("}");
        }
        else
        {
            var tokens = props.Where(
                p => p is AstTokenProperty or AstTokensProperty or AstChildrenProperty { Separated: true }).ToArray();

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

            var children = props.Where(p => p is AstChildProperty or AstChildrenProperty).ToArray();

            writer.Write("public override bool HasChildren");

            if (children.Length == 0)
                writer.WriteLine(" => false;");
            else if (children.Any(p => p is AstChildProperty { Optional: false }))
                writer.WriteLine(" => true;");
            else
            {
                writer.WriteLine();
                writer.WriteLine("{");

                writer.Indent++;

                writer.WriteLine("get");
                writer.WriteLine("{");

                writer.Indent++;

                for (var i = 0; i < children.Length; i++)
                {
                    var prop = children[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case AstChildProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case AstChildrenProperty { Separated: true }:
                            writer.WriteLine($"if ({propName}.Items.Count != 0)");
                            break;
                        default:
                            writer.WriteLine($"if ({propName}.Count != 0)");
                            break;
                    }

                    writer.Indent++;

                    writer.WriteLine("return true;");

                    writer.Indent--;

                    if (i != children.Length - 1)
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
                        case AstTokenProperty { Optional: true } or AstChildProperty { Optional: true }:
                            writer.WriteLine($"{param}?.SetParent(this);");
                            break;
                        case AstTokenProperty or AstChildProperty:
                            writer.WriteLine($"{param}.SetParent(this);");
                            break;
                        default:
                            var sep = prop is AstTokensProperty { Separated: true } or AstChildrenProperty { Separated: true };

                            writer.WriteLine($"foreach (var item in {param}{(sep ? ".Items" : string.Empty)})");

                            writer.Indent++;

                            writer.WriteLine("item.SetParent(this);");

                            writer.Indent--;

                            if (sep)
                            {
                                writer.WriteLine();
                                writer.WriteLine($"foreach (var separator in {param}.Separators)");

                                writer.Indent++;

                                writer.WriteLine("separator.SetParent(this);");

                                writer.Indent--;
                            }

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

            writer.WriteLine("public override IEnumerable<SyntaxItem> Items()");
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
                        case AstTokenProperty { Optional: true } or AstChildProperty { Optional: true }:
                            writer.WriteLine($"if ({propName} != null)");

                            writer.Indent++;

                            writer.WriteLine($"yield return {propName};");

                            writer.Indent--;
                            break;
                        case AstTokenProperty or AstChildProperty:
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

        writer.WriteLine($"public virtual T Visit({tree.GetNodeTypeName(node.Name)} node, T state)");
        writer.WriteLine("{");

        writer.Indent++;

        writer.WriteLine("ArgumentNullException.ThrowIfNull(node);");
        writer.WriteLine();
        writer.WriteLine($"return DefaultVisitNode(node, state);");

        writer.Indent--;

        writer.WriteLine("}");

        writer.Indent--;

        writer.WriteLine("}");

        context.AddSource(name, sb.ToString());
    }
}
