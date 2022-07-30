using Vezel.Celerity.Driver.Commands;
using Vezel.Celerity.Driver.Commands.Format;

var app = new CommandApp<RunCommand>();

app.Configure(cfg =>
{
    // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
#pragma warning disable CS0436
    _ = cfg
        .SetApplicationName(ThisAssembly.AssemblyName)
        .PropagateExceptions();
#pragma warning restore CS0436

    _ = cfg
        .AddCommand<CheckCommand>("check")
        .WithDescription("Perform semantic analysis on Celerity code.");

    cfg.AddBranch("format", format =>
    {
        format.SetDescription("Check or fix Celerity code formatting.");

        _ = format
            .AddCommand<FormatCheckCommand>("check")
            .WithDescription("Check Celerity code formatting.");

        _ = format
            .AddCommand<FormatFixCommand>("fix")
            .WithDescription("Fix Celerity code formatting.");
    });

    _ = cfg
        .AddCommand<InfoCommand>("info")
        .WithDescription("Print Celerity runtime environment information.");

    _ = cfg
        .AddCommand<RunCommand>("run")
        .WithDescription("Run a Celerity program.");

    _ = cfg
        .AddCommand<ServerCommand>("server")
        .WithDescription("Host the Celerity language server.");

    _ = cfg
        .AddCommand<TestCommand>("test")
        .WithDescription("Run unit tests in Celerity code.");
});

return await app.RunAsync(args);
