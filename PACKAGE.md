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
  Provides the language lexer, parser, and abstract syntax tree.
* [Vezel.Celerity.Semantics](https://www.nuget.org/packages/Vezel.Celerity.Semantics):
  Provides semantic analyses such as symbol binding, type checking, and linting.
* [Vezel.Celerity.Runtime](https://www.nuget.org/packages/Vezel.Celerity.Runtime):
  Provides the runtime system consisting of the interpreter, garbage collector,
  agent scheduler, etc.
* [Vezel.Celerity.Kernel](https://www.nuget.org/packages/Vezel.Celerity.Kernel):
  Provides the language's standard library and host operating system interfaces
  for the runtime system.
* [Vezel.Celerity.Server](https://www.nuget.org/packages/Vezel.Celerity.Server):
  Provides the Language Server Protocol implementation.

For more information, please visit the
[project home page](https://docs.vezel.dev/celerity).
