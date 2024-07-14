// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Service;

public static class LanguageService
{
    private sealed class DuplexPipe : IDuplexPipe
    {
        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public DuplexPipe(Stream input, Stream output)
        {
            Input = PipeReader.Create(input, new(leaveOpen: true));
            Output = PipeWriter.Create(output, new(leaveOpen: true));
        }
    }

    public static Task RunAsync(
        LanguageServiceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Check.Null(configuration);

        // TODO: Add cancellation support?
        _ = cancellationToken;

        return RunAsync();

        async Task RunAsync()
        {
            var client = LanguageServer.Connect(new DuplexPipe(configuration.Input, configuration.Output));
            using var server = new CelerityLanguageServer(client);

            await client.RunAsync(server).ConfigureAwait(false);
        }
    }
}
