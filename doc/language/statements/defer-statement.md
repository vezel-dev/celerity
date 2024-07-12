# Defer Statement

```ebnf
defer-statement ::= 'defer' expression ';'
```

Defers evaluation of [`expression`](../expressions.md) (the *body*) until
control leaves the enclosing
[block expression](../expressions/block-expression.md). Multiple `defer`
statements are evaluated in reverse lexical order.

`defer` statements are typically used to reliably clean up resources regardless
of how control leaves a block expression.
