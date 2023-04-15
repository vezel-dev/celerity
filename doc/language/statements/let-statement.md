# Let Statement

```ebnf
let-statement ::= 'let' pattern '=' expression
```

Evaluates [`expression`](../expressions.md) (the *initializer*) and matches it
against [`pattern`](../patterns.md). If the match fails, a panic occurs.

<!-- TODO: Link to panic definition. -->

Any [bindings](../patterns.md#pattern-bindings) in `pattern` are available for
the remainder of the enclosing
[block expression](../expressions/block-expression.md). Any bindings in
`pattern` that existed prior to the `let` statement are shadowed for the
remainder of the enclosing block expression.
