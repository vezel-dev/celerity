namespace Vezel.Celerity.Generators.Syntax;

[Generator]
public sealed class SyntaxTreeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.AdditionalTextsProvider
                .Where(static at => Path.GetFileName(at.Path) == "SyntaxTree.xml"),
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

                SyntaxTreeRoot root;

                using (var stringReader = new StringReader(file.GetText(ctx.CancellationToken)!.ToString()))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                    root = (SyntaxTreeRoot)new XmlSerializer(typeof(SyntaxTreeRoot)).Deserialize(xmlReader);

                var baseName = Path.GetFileName(file.Path);

                foreach (var type in root.Types ?? Array.Empty<SyntaxTreeType>())
                    GenerateType(ctx, Path.ChangeExtension(baseName, $"{type.Name}Syntax.g.cs"), type);
            });
    }

    private static void GenerateType(SourceProductionContext context, string name, SyntaxTreeType type)
    {
        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine("using Vezel.Celerity.Syntax.Tree;");
        writer.WriteLine();
        writer.WriteLine("namespace Vezel.Celerity.Syntax.Tree");
        writer.WriteLine("{");

        writer.Indent++;

        var typeName = $"{type.Name}Syntax";

        writer.Write($"public {(type.Abstract ? "abstract" : "sealed")} partial class ");
        writer.WriteLine($"{typeName} : {(type.Base is { } b ? $"{b}Syntax" : "SyntaxNode")}");
        writer.WriteLine("{");

        writer.Indent++;

        if (!type.Root)
        {
            if (type.Parent is { } p)
                writer.WriteLine($"public new {p}Syntax Parent => Unsafe.As<{p}Syntax>(base.Parent!);");
            else
                writer.WriteLine("public new SyntaxNode Parent => base.Parent!;");

            writer.WriteLine();
        }

        var props = type.Properties ?? Array.Empty<SyntaxTreeProperty>();

        foreach (var prop in props)
        {
            var mod = string.Empty;

            if (type.Abstract)
                mod = " abstract";
            else if (prop.Override)
                mod = " override";

            writer.WriteLine($"public{mod} {prop.GetTypeName()} {prop.GetPropertyName()} {{ get; }}");
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
            var nodes = props.Where(p => p.CanContainNodes).ToArray();

            writer.Write("public override bool HasNodes");

            if (nodes.Length == 0)
                writer.WriteLine(" => false;");
            else if (nodes.Any(p => p is SyntaxTreeNodeProperty { Optional: false }))
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
                        case SyntaxTreeNodeProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case SyntaxTreeNodesProperty p:
                            writer.WriteLine($"if ({propName}{(p.Separated ? ".Elements" : string.Empty)}.Count != 0)");
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

            var tokens = props.Where(p => p.CanContainTokens).ToArray();

            writer.Write("public override bool HasTokens");

            if (tokens.Length == 0)
                writer.WriteLine(" => false;");
            else if (tokens.Any(p => p is SyntaxTreeTokenProperty { Optional: false }))
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
                        case SyntaxTreeNodesProperty:
                            writer.WriteLine($"if ({propName}.Separators.Count != 0)");
                            break;
                        case SyntaxTreeTokenProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case SyntaxTreeTokensProperty:
                            writer.WriteLine($"if ({propName}.Count != 0)");
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

                    writer.WriteLine(
                        $"{prop.GetTypeName()} {prop.GetParameterName()}{(i == props.Length - 1 ? ")" : ",")}");
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
                        case SyntaxTreeNodeProperty { Optional: true } or SyntaxTreeTokenProperty { Optional: true }:
                            writer.WriteLine($"{param}?.SetParent(this);");
                            break;
                        case SyntaxTreeNodeProperty or SyntaxTreeTokenProperty:
                            writer.WriteLine($"{param}.SetParent(this);");
                            break;
                        case SyntaxTreeNodesProperty or SyntaxTreeTokensProperty:
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
                        case SyntaxTreeNodeProperty { Optional: true } or SyntaxTreeTokenProperty { Optional: true }:
                            writer.WriteLine($"if ({propName} != null)");

                            writer.Indent++;

                            writer.WriteLine($"yield return {propName};");

                            writer.Indent--;
                            break;
                        case SyntaxTreeNodeProperty or SyntaxTreeTokenProperty:
                            writer.WriteLine($"yield return {propName};");
                            break;
                        case SyntaxTreeNodesProperty or SyntaxTreeTokensProperty:
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

        writer.Indent--;

        writer.WriteLine("}");

        if (!type.Abstract)
        {
            writer.WriteLine();
            writer.WriteLine("namespace Vezel.Celerity.Syntax");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("public abstract partial class SyntaxWalker<T>");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"public virtual T Visit({type.Name}Syntax node, T state)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("Check.Null(node);");
            writer.WriteLine();
            writer.WriteLine("return DefaultVisitNode(node, state);");

            writer.Indent--;

            writer.WriteLine("}");

            writer.Indent--;

            writer.WriteLine("}");

            writer.Indent--;

            writer.WriteLine("}");
        }

        context.AddSource(name, sb.ToString());
    }
}
