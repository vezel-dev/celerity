namespace Vezel.Celerity.Generators.Syntax;

[Generator]
public sealed class SyntaxTreeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.AdditionalTextsProvider
                .Where(static at => Path.GetFileName(at.Path) == "SyntaxTree.xml"),
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

                SyntaxTreeRoot root;

                using (var stringReader = new StringReader(file.GetText(ctx.CancellationToken)!.ToString()))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                    root = (SyntaxTreeRoot)new XmlSerializer(typeof(SyntaxTreeRoot)).Deserialize(xmlReader);

                var baseName = Path.GetFileName(file.Path);

                foreach (var type in root.Types ?? [])
                    GenerateType(ctx, Path.ChangeExtension(baseName, $"{type.Name}Syntax.g.cs"), type);
            });
    }

    private static void GenerateType(SourceProductionContext context, string name, SyntaxTreeType type)
    {
        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine("using Vezel.Celerity.Language.Syntax.Tree;");
        writer.WriteLine();
        writer.WriteLine("namespace Vezel.Celerity.Language.Syntax.Tree");
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

        var props = type.Properties ?? [];

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
            var nodes = props.Where(static p => p.CanContainNodes).ToArray();

            writer.Write("public override bool HasNodes");

            if (nodes.Length == 0)
                writer.WriteLine(" => false;");
            else if (nodes.Any(static p => p is SyntaxTreeNodeProperty { Optional: false }))
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
                        case SyntaxTreeNodesProperty { Separated: true } p:
                            writer.WriteLine($"if ({propName}.Elements.Length != 0)");
                            break;
                        case SyntaxTreeNodesProperty p:
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

            var tokens = props.Where(p => p.CanContainTokens).ToArray();

            writer.Write("public override bool HasTokens");

            if (tokens.Length == 0)
                writer.WriteLine(" => false;");
            else if (tokens.Any(static p => p is SyntaxTreeTokenProperty { Optional: false }))
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
                            writer.WriteLine($"if ({propName}.Separators.Length != 0)");
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
                {
                    var param = prop.GetParameterName();

                    writer.Write($"{prop.GetPropertyName()} = ");
                    writer.WriteLine(
                        prop is SyntaxTreeNodesProperty or SyntaxTreeTokensProperty
                            ? $"new({param}, this);"
                            : $"{param};");
                }

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
                    }
                }

                writer.WriteLine();
                writer.WriteLine("Initialize();");

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

            writer.WriteLine("public override IEnumerable<SyntaxNode> ChildNodes()");
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
                        case SyntaxTreeNodeProperty p:
                            if (p.Optional)
                            {
                                writer.WriteLine($"if ({propName} != null)");

                                writer.Indent++;
                            }

                            writer.WriteLine($"yield return {propName};");

                            if (p.Optional)
                                writer.Indent--;

                            break;
                        case SyntaxTreeNodesProperty p:
                            writer.WriteLine(
                                $"foreach (var item in {propName}{(p.Separated ? ".Elements" : string.Empty)})");

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
                writer.WriteLine("return Array.Empty<SyntaxNode>();");

            writer.Indent--;

            writer.WriteLine("}");

            writer.WriteLine();

            writer.WriteLine("public override IEnumerable<SyntaxToken> ChildTokens()");
            writer.WriteLine("{");

            writer.Indent++;

            if (tokens.Length != 0)
            {
                for (var i = 0; i < tokens.Length; i++)
                {
                    var prop = tokens[i];
                    var propName = prop.GetPropertyName();

                    switch (prop)
                    {
                        case SyntaxTreeNodesProperty:
                            writer.WriteLine($"foreach (var item in {propName}.Separators)");

                            writer.Indent++;

                            writer.WriteLine("yield return item;");

                            writer.Indent--;
                            break;
                        case SyntaxTreeTokenProperty p:
                            if (p.Optional)
                            {
                                writer.WriteLine($"if ({propName} != null)");

                                writer.Indent++;
                            }

                            writer.WriteLine($"yield return {propName};");

                            if (p.Optional)
                                writer.Indent--;

                            break;
                        case SyntaxTreeTokensProperty p:
                            writer.WriteLine($"foreach (var item in {propName})");

                            writer.Indent++;

                            if (p.Separated)
                                writer.WriteLine("yield return Unsafe.As<SyntaxToken>(item);");
                            else
                                writer.WriteLine("yield return item;");

                            writer.Indent--;
                            break;
                    }

                    if (i != tokens.Length - 1)
                        writer.WriteLine();
                }
            }
            else
                writer.WriteLine("return Array.Empty<SyntaxToken>();");

            writer.Indent--;

            writer.WriteLine("}");

            writer.WriteLine();

            writer.WriteLine("internal override void Visit(SyntaxVisitor visitor)");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"visitor.Visit{type.Name}(this);");

            writer.Indent--;

            writer.WriteLine("}");

            writer.WriteLine();

            writer.WriteLine("internal override T? Visit<T>(SyntaxVisitor<T> visitor)");

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
            writer.WriteLine("namespace Vezel.Celerity.Language.Syntax");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine("public abstract partial class SyntaxVisitor");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"public virtual void Visit{type.Name}({type.Name}Syntax node)");
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
            writer.WriteLine("public abstract partial class SyntaxVisitor<T>");
            writer.WriteLine("{");

            writer.Indent++;

            writer.WriteLine($"public virtual T? Visit{type.Name}({type.Name}Syntax node)");
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
