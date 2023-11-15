namespace Vezel.Celerity.Generators.Semantics;

[Generator]
public sealed class SemanticTreeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.AdditionalTextsProvider
                .Where(static at => Path.GetFileName(at.Path) == "SemanticTree.xml"),
            static (ctx, file) =>
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

                SemanticTreeRoot root;

                using (var stringReader = new StringReader(file.GetText(ctx.CancellationToken)!.ToString()))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                    root = (SemanticTreeRoot)new XmlSerializer(typeof(SemanticTreeRoot)).Deserialize(xmlReader);

                var baseName = Path.GetFileName(file.Path);

                foreach (var type in root.Types ?? [])
                    GenerateType(ctx, Path.ChangeExtension(baseName, $"{type.Name}Semantics.g.cs"), type);
            });
    }

    private static void GenerateType(SourceProductionContext context, string name, SemanticTreeType type)
    {
        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine("using Vezel.Celerity.Language.Semantics.Binding;");
        writer.WriteLine("using Vezel.Celerity.Language.Semantics.Tree;");
        writer.WriteLine("using Vezel.Celerity.Language.Syntax.Tree;");
        writer.WriteLine();
        writer.WriteLine("namespace Vezel.Celerity.Language.Semantics.Tree");
        writer.WriteLine("{");

        writer.Indent++;

        var typeName = $"{type.Name}Semantics";

        writer.Write($"public {(type.Abstract ? "abstract" : "sealed")} partial class ");
        writer.WriteLine($"{typeName} : {(type.Base is { } b ? $"{b}Semantics" : "SemanticNode")}");
        writer.WriteLine("{");

        writer.Indent++;

        var syntaxTypeName = $"{type.Name}Syntax";

        writer.WriteLine($"public new {syntaxTypeName} Syntax => Unsafe.As<{syntaxTypeName}>(base.Syntax);");
        writer.WriteLine();

        if (!type.Root)
        {
            if (type.Parent is { } p)
                writer.WriteLine($"public new {p}Semantics Parent => Unsafe.As<{p}Semantics>(base.Parent!);");
            else
                writer.WriteLine("public new SemanticNode Parent => base.Parent!;");

            writer.WriteLine();
        }

        var props = type.Properties ?? [];

        foreach (var prop in props)
        {
            var propType = prop.GetTypeName();
            var propName = prop.GetPropertyName();

            writer.Write("public ");

            if (prop is SemanticTreeComputedProperty comp)
                writer.WriteLine($"{propType} {propName} => {comp.Body};");
            else
            {
                var mod = string.Empty;

                if (type.Abstract)
                    mod = "abstract ";
                else if (prop.Override)
                    mod = "override ";

                writer.WriteLine($"{mod}{propType} {propName} {{ get; }}");
            }

            writer.WriteLine();
        }

        if (type.Abstract)
        {
            writer.WriteLine($"private protected {typeName}({syntaxTypeName} syntax)");

            writer.Indent++;

            writer.WriteLine(": base(syntax)");

            writer.Indent--;

            writer.WriteLine("{");
            writer.WriteLine("}");
        }
        else
        {
            var nodes = props.Where(static p => p is SemanticTreeNodeProperty or SemanticTreeNodesProperty).ToArray();

            writer.Write("public override bool HasChildren");

            if (nodes.Length == 0)
                writer.WriteLine(" => false;");
            else if (nodes.Any(static p => p is SemanticTreeNodeProperty { Optional: false }))
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
                        case SemanticTreeNodeProperty:
                            writer.WriteLine($"if ({propName} != null)");
                            break;
                        case SemanticTreeNodesProperty:
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

            var fields = props.Where(static p => p is not SemanticTreeComputedProperty).ToArray();

            if (fields.Length != 0)
            {
                writer.WriteLine($"internal {typeName}(");

                writer.Indent++;

                writer.WriteLine($"{syntaxTypeName} syntax,");

                for (var i = 0; i < fields.Length; i++)
                {
                    var prop = fields[i];

                    writer.WriteLine(
                        $"{prop.GetTypeName()} {prop.GetParameterName()}{(i == fields.Length - 1 ? ")" : ",")}");
                }

                writer.WriteLine(": base(syntax)");

                writer.Indent--;

                writer.WriteLine("{");

                writer.Indent++;

                foreach (var prop in fields)
                {
                    var param = prop.GetParameterName();

                    writer.Write(
                        $"{prop.GetPropertyName()} = ");
                    writer.WriteLine(prop is SemanticTreeNodesProperty ? $"new({param}, this);" : $"{param};");
                }

                foreach (var prop in fields.OfType<SemanticTreeNodeProperty>())
                {
                    writer.WriteLine();
                    writer.WriteLine(
                        $"{prop.GetParameterName()}{(prop.Optional ? "?" : string.Empty)}.SetParent(this);");
                }

                writer.Indent--;

                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine($"internal {typeName}({syntaxTypeName} syntax)");

                writer.Indent++;

                writer.WriteLine(": base(syntax)");

                writer.Indent--;

                writer.WriteLine("{");
                writer.WriteLine("}");
            }

            writer.WriteLine();

            writer.WriteLine("public override IEnumerable<SemanticNode> Children()");
            writer.WriteLine("{");

            writer.Indent++;

            if (nodes.Length != 0)
            {
                for (var i = 0; i < nodes.Length; i++)
                {
                    var prop = nodes[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case SemanticTreeNodeProperty p:
                            if (p.Optional)
                            {
                                writer.WriteLine($"if ({propName} != null)");

                                writer.Indent++;
                            }

                            writer.WriteLine($"yield return {propName};");

                            if (p.Optional)
                                writer.Indent--;

                            break;
                        default:
                            writer.WriteLine($"foreach (var item in {propName})");

                            writer.Indent++;

                            writer.WriteLine("yield return item;");

                            writer.Indent--;
                            break;
                    }

                    if (i != nodes.Length - 1)
                        writer.WriteLine();
                }
            }
            else
                writer.WriteLine("return Array.Empty<SemanticNode>();");

            writer.Indent--;

            writer.WriteLine("}");

            writer.WriteLine();

            writer.WriteLine("internal override void Visit(SemanticVisitor visitor)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"visitor.Visit{type.Name}(this);");

            writer.Indent--;

            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("internal override T? Visit<T>(SemanticVisitor<T> visitor)");

            writer.Indent++;

            writer.WriteLine("where T : default");

            writer.Indent--;

            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"return visitor.Visit{type.Name}(this);");

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
            writer.WriteLine("namespace Vezel.Celerity.Language.Semantics");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("public abstract partial class SemanticVisitor");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"public virtual void Visit{type.Name}({type.Name}Semantics node)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("Check.Null(node);");
            writer.WriteLine();
            writer.WriteLine("DefaultVisit(node);");

            writer.Indent--;

            writer.WriteLine("}");

            writer.Indent--;

            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("public abstract partial class SemanticVisitor<T>");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"public virtual T? Visit{type.Name}({type.Name}Semantics node)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("Check.Null(node);");
            writer.WriteLine();
            writer.WriteLine("return DefaultVisit(node);");

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
