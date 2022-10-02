namespace Vezel.Celerity.Tests.Adapter;

internal static partial class CelerityTestLoader
{
    private const string DirectiveStarter = "//";

    public static IReadOnlyDictionary<string, CelerityTestCase> Tests { get; }

    static CelerityTestLoader()
    {
        var tests = new Dictionary<string, CelerityTestCase>();

        static string TryReadFile(string path)
        {
            try
            {
                return File.ReadAllText(path).ReplaceLineEndings().Trim();
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
        }

        var dir = Path.Combine(Path.GetDirectoryName(typeof(ThisAssembly).Assembly.Location)!, "..", "..");

        foreach (var file in new DirectoryInfo(dir).EnumerateFiles("*.cel"))
        {
            var cmd = "run";
            var args = string.Empty;
            var env = new Dictionary<string, string>();
            var exp = true;

            foreach (var line in File.ReadAllLines(file.FullName))
            {
                var comment = line.Trim();

                if (!comment.StartsWith(DirectiveStarter, StringComparison.Ordinal))
                    break;

                var directive = comment[DirectiveStarter.Length..].TrimStart();

                if (directive == "test:pass")
                    exp = true;
                else if (directive == "test:fail")
                    exp = false;
                else if (CommandRegex().Match(directive) is { Success: true } cmdMatch)
                    cmd += $" {cmdMatch.Groups[1].Value}";
                else if (ArgumentsRegex().Match(directive) is { Success: true } argsMatch)
                    args += $" {argsMatch.Groups[1].Value}";
                else if (EnvironmentRegex().Match(directive) is { Success: true } envMatch)
                    env[envMatch.Groups[1].Value] = envMatch.Groups[2].Value;
            }

            var tc = new CelerityTestCase(
                Path.GetFileNameWithoutExtension(file.Name)!,
                file,
                cmd,
                args,
                env,
                exp,
                TryReadFile(Path.ChangeExtension(file.FullName, "out")!),
                TryReadFile(Path.ChangeExtension(file.FullName, "err")!));

            tests.Add(tc.FullName, tc);
        }

        Tests = tests;
    }

    [GeneratedRegex(@"^test:cmd (.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex CommandRegex();

    [GeneratedRegex(@"^test:args (.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ArgumentsRegex();

    [GeneratedRegex(@"^test:env (.*)=(.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex EnvironmentRegex();
}
