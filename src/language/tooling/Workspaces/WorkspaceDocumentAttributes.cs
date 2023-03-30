namespace Vezel.Celerity.Language.Tooling.Workspaces;

[Flags]
public enum WorkspaceDocumentAttributes
{
    None = 0b000,
    EntryPoint = 0b001,
    DisableAnalyzers = 0b010,
    SuppressDiagnostics = 0b100,
}
