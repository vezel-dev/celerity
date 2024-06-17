# Celerity

<div align="center">
    <img src="celerity.svg"
         width="128"
         alt="Celerity" />
</div>

<p align="center">
    <strong>
        An expressive programming language for writing concurrent and
        maintainable software.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/vezel-dev/celerity?color=brown)](LICENSE-0BSD)
[![Commits](https://img.shields.io/github/commit-activity/m/vezel-dev/celerity/master?label=commits&color=slateblue)](https://github.com/vezel-dev/celerity/commits/master)
[![Build](https://img.shields.io/github/actions/workflow/status/vezel-dev/celerity/build.yml?branch=master)](https://github.com/vezel-dev/celerity/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/celerity?color=teal)](https://github.com/vezel-dev/celerity/discussions)
[![Discord](https://img.shields.io/badge/discord-chat-7289da?logo=discord)](https://discord.gg/wtzCfaX2Nj)

</div>

--------------------------------------------------------------------------------

> [!WARNING]
> This is currently in-development vaporware.

**Celerity** is a programming language aiming for a good balance of
productivity and scalability while being easily embeddable in host applications.

**Celerity** is expression-oriented, multi-paradigm, and features optional type
checking. Some notable features are pattern matching, first-class functions with
closures, opt-in mutability, explicit yet terse error propagation, concurrency
based on lightweight agents, and non-suspending garbage collection.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![celerity][driver-img]][driver-pkg] | Provides the .NET global tool. | ![Downloads][driver-dls] |
| [![Vezel.Celerity.Language][language-core-img]][language-core-pkg] | Provides language analysis services such as lexing, parsing, binding, and linting. | ![Downloads][language-core-dls] |
| [![Vezel.Celerity.Language.Library][language-library-img]][language-library-pkg] | Provides the language's standard library. | ![Downloads][language-library-dls] |
| [![Vezel.Celerity.Language.Tooling][language-tooling-img]][language-tooling-pkg] | Provides user-oriented tooling such as project management and diagnostic rendering. | ![Downloads][language-tooling-dls] |
| [![Vezel.Celerity.Language.Service][language-service-img]][language-service-pkg] | Provides the Language Server Protocol implementation. | ![Downloads][language-service-dls] |
| [![Vezel.Celerity.Runtime][runtime-core-img]][runtime-core-pkg] | Provides shared components of the runtime system such as the bytecode lowerer, garbage collector, and agent scheduler. | ![Downloads][runtime-core-dls] |
| [![Vezel.Celerity.Runtime.Kernel][runtime-kernel-img]][runtime-kernel-pkg] | Provides the host operating system interfaces for the runtime system. | ![Downloads][runtime-kernel-dls] |
| [![Vezel.Celerity.Runtime.Interpreter][runtime-interpreter-img]][runtime-interpreter-pkg] | Provides the portable bytecode interpreter which does not require dynamic code generation. | ![Downloads][runtime-interpreter-dls] |
| [![Vezel.Celerity.Runtime.Compiler][runtime-compiler-img]][runtime-compiler-pkg] | Provides the optimizing just-in-time compiler for 64-bit systems. | ![Downloads][runtime-compiler-dls] |

[driver-pkg]: https://www.nuget.org/packages/celerity
[language-core-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Language
[language-library-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Language.Library
[language-tooling-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Language.Tooling
[language-service-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Language.Service
[runtime-core-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime
[runtime-kernel-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime.Kernel
[runtime-interpreter-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime.Interpreter
[runtime-compiler-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime.Compiler

[driver-img]: https://img.shields.io/nuget/v/celerity?label=celerity
[language-core-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Language?label=Vezel.Celerity.Language
[language-library-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Language.Library?label=Vezel.Celerity.Language.Library
[language-tooling-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Language.Tooling?label=Vezel.Celerity.Language.Tooling
[language-service-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Language.Service?label=Vezel.Celerity.Language.Service
[runtime-core-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime?label=Vezel.Celerity.Runtime
[runtime-kernel-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime.Kernel?label=Vezel.Celerity.Runtime.Kernel
[runtime-interpreter-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime.Interpreter?label=Vezel.Celerity.Runtime.Interpreter
[runtime-compiler-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime.Compiler?label=Vezel.Celerity.Runtime.Compiler

[driver-dls]: https://img.shields.io/nuget/dt/celerity?label=
[language-core-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Language?label=
[language-library-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Language.Library?label=
[language-tooling-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Language.Tooling?label=
[language-service-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Language.Service?label=
[runtime-core-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime?label=
[runtime-kernel-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime.Kernel?label=
[runtime-interpreter-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime.Interpreter?label=
[runtime-compiler-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime.Compiler?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).

## Building

You will need the .NET SDK and Node.js installed. Simply run `./cake`
(a [Bash](https://www.gnu.org/software/bash) script) to build artifacts and run
tests. You can also run `./cake pack` to avoid running tests, or `./cake test`
to just build and run tests.

These commands will use the `Debug` configuration by default, which is suitable
for development and debugging. Pass `-c Release` instead to get an optimized
build.

## License

This project is licensed under the terms found in
[`LICENSE-0BSD`](LICENSE-0BSD).
