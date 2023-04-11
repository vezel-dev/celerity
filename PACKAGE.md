# Celerity

**Warning:** This is currently in-development vaporware.

**Celerity** is a programming language aiming for a good balance of
productivity and scalability while being easily embeddable in host applications.

**Celerity** is expression-oriented, multi-paradigm, and features optional type
checking. Some notable features are pattern matching, first-class functions with
closures, opt-in mutability, explicit yet terse error propagation, concurrency
based on lightweight agents, and non-suspending garbage collection.

This project offers the following packages:

* [celerity](https://www.nuget.org/packages/celerity): Provides the .NET global
  tool.
* [Vezel.Celerity.Language](https://www.nuget.org/packages/Vezel.Celerity.Language):
  Provides language analysis services such as lexing, parsing, binding, and
  linting.
* [Vezel.Celerity.Language.Library](https://www.nuget.org/packages/Vezel.Celerity.Language.Library):
  Provides the language's standard library.
* [Vezel.Celerity.Language.Tooling](https://www.nuget.org/packages/Vezel.Celerity.Language.Tooling):
  Provides user-oriented tooling such as project management and diagnostic
  rendering.
* [Vezel.Celerity.Language.Service](https://www.nuget.org/packages/Vezel.Celerity.Language.Service):
  Provides the Language Server Protocol implementation.
* [Vezel.Celerity.Runtime](https://www.nuget.org/packages/Vezel.Celerity.Runtime):
  Provides shared components of the runtime system such as the bytecode lowerer,
  garbage collector, and agent scheduler.
* [Vezel.Celerity.Runtime.Kernel](https://www.nuget.org/packages/Vezel.Celerity.Runtime.Kernel):
  Provides the host operating system interfaces for the runtime system.
* [Vezel.Celerity.Runtime.Interpreter](https://www.nuget.org/packages/Vezel.Celerity.Runtime.Interpreter):
  Provides the portable bytecode interpreter which does not require dynamic code
  generation.
* [Vezel.Celerity.Runtime.Compiler](https://www.nuget.org/packages/Vezel.Celerity.Runtime.Compiler):
  Provides the optimizing just-in-time compiler for 64-bit systems.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).
