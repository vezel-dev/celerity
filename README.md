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
[![Build](https://img.shields.io/github/workflow/status/vezel-dev/celerity/Build/master)](https://github.com/vezel-dev/celerity/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/celerity?color=teal)](https://github.com/vezel-dev/celerity/discussions)
[![Discord](https://img.shields.io/discord/960716713136095232?color=peru&label=discord)](https://discord.gg/SdBCrRuNxY)

</div>

---

**Warning:** This is currently in-development vaporware.

**Celerity** is a programming language aiming for a good balance of
productivity and scalability while being easily embeddable in host applications.

**Celerity** is expression-oriented, multi-paradigm, and features optional type
checking through success typings. Some notable features are pattern matching,
first-class functions with closures, opt-in mutability, explicit exception
propagation, concurrency based on lightweight agents, and non-suspending garbage
collection.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![celerity][cli-img]][cli-pkg] | Provides the .NET global tool. | ![Downloads][cli-dls] |
| [![Vezel.Celerity.Common][common-img]][common-pkg] | Provides common functionality used by all Celerity packages. | ![Downloads][common-dls] |
| [![Vezel.Celerity.Syntax][syntax-img]][syntax-pkg] | Provides the language lexer, parser, and abstract syntax tree. | ![Downloads][syntax-dls] |
| [![Vezel.Celerity.Semantics][semantics-img]][semantics-pkg] | Provides semantic analyses such as symbol binding, type checking, and linting. | ![Downloads][semantics-dls] |
| [![Vezel.Celerity.Runtime][runtime-img]][runtime-pkg] | Provides the runtime system consisting of the interpreter, garbage collector, agent scheduler, etc. | ![Downloads][runtime-dls] |
| [![Vezel.Celerity.Kernel][kernel-img]][kernel-pkg] | Provides the language's standard library and host operating system interfaces for the runtime system. | ![Downloads][kernel-dls] |
| [![Vezel.Celerity.Server][server-img]][server-pkg] | Provides the Language Server Protocol implementation. | ![Downloads][server-dls] |

[cli-pkg]: https://www.nuget.org/packages/celerity
[common-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Common
[syntax-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Syntax
[semantics-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Semantics
[runtime-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Runtime
[kernel-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Kernel
[server-pkg]: https://www.nuget.org/packages/Vezel.Celerity.Server

[cli-img]: https://img.shields.io/nuget/v/celerity?label=celerity
[common-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Common?label=Vezel.Celerity.Common
[syntax-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Syntax?label=Vezel.Celerity.Syntax
[semantics-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Semantics?label=Vezel.Celerity.Semantics
[runtime-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Runtime?label=Vezel.Celerity.Runtime
[kernel-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Kernel?label=Vezel.Celerity.Kernel
[server-img]: https://img.shields.io/nuget/v/Vezel.Celerity.Server?label=Vezel.Celerity.Server

[cli-dls]: https://img.shields.io/nuget/dt/celerity?label=
[common-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Common?label=
[syntax-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Syntax?label=
[semantics-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Semantics?label=
[runtime-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Runtime?label=
[kernel-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Kernel?label=
[server-dls]: https://img.shields.io/nuget/dt/Vezel.Celerity.Server?label=

To install a tool package in a project, run `dotnet tool install <name>`. To
install it globally, also pass `-g`.

To install a library package, run `dotnet add package <name>`.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).

## License

This project is licensed under the terms found in
[`LICENSE-0BSD`](LICENSE-0BSD).
