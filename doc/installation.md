# Installation

Celerity can be installed as a
[.NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) in
two ways:

* To install Celerity globally, run `dotnet tool install celerity -g`. This will
  allow you to run the `celerity` CLI driver anywhere, assuming you have
  [set up your `PATH` appropriately](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool).
* To install Celerity locally in a .NET-based project with a
  [tool manifest](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use#create-a-manifest-file),
  omit the `-g` option. This will require you to run `dotnet celerity` from the
  project directory.

<!-- TODO: Add more instructions once we publish signed binaries. -->
