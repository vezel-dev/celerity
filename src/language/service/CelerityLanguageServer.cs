// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language;

internal sealed class CelerityLanguageServer : ILanguageServer
{
    public InitializeResult.ServerInfoResult Info { get; } = new()
    {
        Name = typeof(ThisAssembly).Assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product,
        Version = ThisAssembly.AssemblyInformationalVersion,
    };

    private readonly ILanguageClient _client;

    public CelerityLanguageServer(ILanguageClient client)
    {
        _client = client;

        _ = _client; // TODO
    }

    public void Dispose()
    {
    }

    public Task InitializeAsync(InitializeParams param)
    {
        // TODO: Initialize the workspace.
        return Task.CompletedTask;
    }

    public Task InitializedAsync(InitializedParams param)
    {
        // TODO: Do we need to do anything here?
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        // TODO: Clean up any resources.
        return Task.CompletedTask;
    }
}
