// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Service;

public sealed class LanguageServiceConfiguration
{
    public Stream Input { get; private set; }

    public Stream Output { get; private set; }

    private LanguageServiceConfiguration()
    {
        Input = null!;
        Output = null!;
    }

    public LanguageServiceConfiguration(Stream input, Stream output)
    {
        Check.Null(input);
        Check.Argument(input.CanRead, input);
        Check.Null(output);
        Check.Argument(output.CanWrite, output);

        Input = input;
        Output = output;
    }

    private LanguageServiceConfiguration Clone()
    {
        return new()
        {
            Input = Input,
            Output = Output,
        };
    }

    public LanguageServiceConfiguration WithInput(Stream input)
    {
        Check.Null(input);
        Check.Argument(input.CanRead, input);

        var cfg = Clone();

        cfg.Input = input;

        return cfg;
    }

    public LanguageServiceConfiguration WithOutput(Stream output)
    {
        Check.Null(output);
        Check.Argument(output.CanWrite, output);

        var cfg = Clone();

        cfg.Output = output;

        return cfg;
    }
}
