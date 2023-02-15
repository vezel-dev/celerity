# Celerity

<div align="center">
    <img src="celerity.svg"
         width="128" />
</div>

<p align="center">
    <strong>
        A highly concurrent and expressive programming language suitable for
        embedding.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/vezel-dev/celerity?color=brown)](LICENSE-0BSD)
[![Commits](https://img.shields.io/github/commit-activity/m/vezel-dev/celerity/master?label=commits&color=slateblue)](https://github.com/vezel-dev/celerity/commits/master)
[![Build](https://img.shields.io/github/actions/workflow/status/vezel-dev/celerity/build.yml?branch=master)](https://github.com/vezel-dev/celerity/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/celerity?color=teal)](https://github.com/vezel-dev/celerity/discussions)
[![Discord](https://img.shields.io/discord/960716713136095232?color=peru&label=discord)](https://discord.gg/uD8maMVVFX)

</div>

---

**Warning:** This is currently in-development vaporware.

**Celerity** is a programming language aiming for a good balance of
productivity and scalability while being easily embeddable in host applications.

**Celerity** is expression-oriented, multi-paradigm, and features optional type
checking through success typings. Some notable features are pattern matching,
first-class functions with closures, opt-in mutability, explicit error
propagation, concurrency based on lightweight agents, and non-suspending garbage
collection.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![celerity][cli-img]][cli-pkg] | Provides the .NET global tool. | ![Downloads][cli-dls] |
| [![Vezel.Celerity.Syntax][syntax-img]][syntax-pkg] | Provides syntactical analyses such as lexing and parsing, as well as the syntax tree representation. | ![Downloads][syntax-dls] |
| [![Vezel.Celerity.Semantics][semantics-img]][semantics-pkg] | Provides semantic analyses such as symbol binding and type checking, as well as the semantic tree representation. | ![Downloads][semantics-dls] |
| [![Vezel.Celerity.Runtime][runtime-img]][runtime-pkg] | Provides shared components of the runtime system such as the bytecode lowerer, garbage collector, and agent scheduler. | ![Downloads][runtime-dls] |
| [![Vezel.Celerity.Interpreter][interpreter-img]][interpreter-pkg] | Provides the portable bytecode interpreter which does not require dynamic code generation or the supporting native library. | ![Downloads][interpreter-dls] |
| [![Vezel.Celerity.Jit][jit-img]][jit-pkg] | Provides the optimizing just-in-time compiler for 64-bit systems, as well as the supporting native library. | ![Downloads][jit-dls] |
| [![Vezel.Celerity.Kernel][kernel-img]][kernel-pkg] | Provides the language's standard library and host operating system interfaces for the runtime system. | ![Downloads][kernel-dls] |
| [![Vezel.Celerity.Tooling][tooling-img]][tooling-pkg] | Provides high-level tooling such as diagnostic rendering and source code formatting. | ![Downloads][tooling-dls] |
| [![Vezel.Celerity.Server][server-img]][server-pkg] | Provides the Language Server Protocol implementation. | ![Downloads][server-dls] |

[cli-pkg]: https://www.nuget.org/packages/celerity
[syntax-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Syntax
[semantics-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Semantics
[runtime-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime
[interpreter-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Interpreter
[jit-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Jit
[kernel-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Kernel
[tooling-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Tooling
[server-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Server

[cli-img]: https://img.shields.io/nuget/v/celerity?label=celerity
[syntax-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Syntax?label=Vezel.Celerity.Syntax
[semantics-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Semantics?label=Vezel.Celerity.Semantics
[runtime-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime?label=Vezel.Celerity.Runtime
[interpreter-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Interpreter?label=Vezel.Celerity.Interpreter
[jit-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Jit?label=Vezel.Celerity.Jit
[kernel-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Kernel?label=Vezel.Celerity.Kernel
[tooling-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Tooling?label=Vezel.Celerity.Tooling
[server-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Server?label=Vezel.Celerity.Server

[cli-dls]: https://img.shields.io/nuget/dt/celerity?label=
[syntax-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Syntax?label=
[semantics-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Semantics?label=
[runtime-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime?label=
[interpreter-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Interpreter?label=
[jit-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Jit?label=
[kernel-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Kernel?label=
[tooling-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Tooling?label=
[server-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Server?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).

## License

This project is licensed under the terms found in
[`LICENSE-0BSD`](LICENSE-0BSD).
