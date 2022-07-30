using Vezel.Celerity.Benchmarks.Commands;

var app = new CommandApp<BenchmarkCommand>();

app.Configure(cfg =>
    cfg
        .SetApplicationName(ThisAssembly.AssemblyName)
        .PropagateExceptions());

return app.Run(args);
