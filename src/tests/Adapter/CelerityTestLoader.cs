namespace Vezel.Celerity.Tests.Adapter;

internal static partial class CelerityTestLoader
{
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
            var args = string.Empty;
            var env = new Dictionary<string, string>();
            var exp = true;

            foreach (var line in File.ReadAllLines(file.FullName))
            {
                if (!line.StartsWith('#'))
                    break;

                var directive = line[1..].Trim();

                Match m;

                if (directive == "test:pass")
                    exp = true;
                else if (directive == "test:fail")
                    exp = false;
                else if ((m = ArgumentsRegex().Match(directive)).Success)
                    args += m.Groups[1].Value;
                else if ((m = EnvironmentRegex().Match(directive)).Success)
                    args = env[m.Groups[1].Value] = m.Groups[2].Value;
            }

            var tc = new CelerityTestCase(
                Path.GetFileNameWithoutExtension(file.Name)!,
                file,
                args,
                env,
                exp,
                TryReadFile(Path.ChangeExtension(file.FullName, "out")!),
                TryReadFile(Path.ChangeExtension(file.FullName, "err")!));

            tests.Add(tc.FullName, tc);
        }

        Tests = tests;
    }

    [RegexGenerator(@"^test:env (.*)=(.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex EnvironmentRegex();

    [RegexGenerator(@"^test:args (.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ArgumentsRegex();
}
