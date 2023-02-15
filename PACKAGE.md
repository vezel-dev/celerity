# Celerity

**Warning:** This is currently in-development vaporware.

**Celerity** is a programming language aiming for a good balance of
productivity and scalability while being easily embeddable in host applications.

**Celerity** is expression-oriented, multi-paradigm, and features optional type
checking through success typings. Some notable features are pattern matching,
first-class functions with closures, opt-in mutability, explicit error
propagation, concurrency based on lightweight agents, and non-suspending garbage
collection.

This project offers the following packages:

* [celerity](https://www.nuget.org/packages/celerity): Provides the .NET global
  tool.
* [Vezel.Celerity.Syntax](https://www.nuget.org/packages/Vezel.Celerity.Syntax):
  Provides syntactical analyses such as lexing and parsing, as well as the
  syntax tree representation.
* [Vezel.Celerity.Semantics](https://www.nuget.org/packages/Vezel.Celerity.Semantics):
  Provides semantic analyses such as symbol binding and type checking, as well
  as the semantic tree representation.
* [Vezel.Celerity.Runtime](https://www.nuget.org/packages/Vezel.Celerity.Runtime):
  Provides shared components of the runtime system such as the bytecode lowerer,
  garbage collector, and agent scheduler.
* [Vezel.Celerity.Interpreter](https://www.nuget.org/packages/Vezel.Celerity.Interpreter):
  Provides the portable bytecode interpreter which does not require dynamic code
  generation or the supporting native library.
* [Vezel.Celerity.Jit](https://www.nuget.org/packages/Vezel.Celerity.Jit):
  Provides the optimizing just-in-time compiler for 64-bit systems, as well as
  the supporting native library.
* [Vezel.Celerity.Kernel](https://www.nuget.org/packages/Vezel.Celerity.Kernel):
  Provides the language's standard library and host operating system interfaces
  for the runtime system.
* [Vezel.Celerity.Tooling](https://www.nuget.org/packages/Vezel.Celerity.Tooling):
  Provides high-level tooling such as diagnostic rendering and source code
  formatting.
* [Vezel.Celerity.Server](https://www.nuget.org/packages/Vezel.Celerity.Server):
  Provides the Language Server Protocol implementation.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).
