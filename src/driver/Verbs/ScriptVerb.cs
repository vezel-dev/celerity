// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Driver.Workspaces;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("script", isDefault: true, Hidden = true, HelpText = "Run a Celerity script.")]
internal sealed class ScriptVerb : Verb
{
    [Value(0, Required = true, HelpText = "Script file.")]
    public required string File { get; init; }

    [Value(1, HelpText = "Script arguments.")]
    public required IEnumerable<string> Arguments { get; init; }

    protected override ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(File) || Path.GetExtension(File) != ".cel")
            throw new DriverException($"Invalid script path '{File}'.");

        var workspace = new ScriptWorkspace(File);

        // TODO: Run the script.
        _ = workspace;

        return ValueTask.FromResult(0);
    }
}
