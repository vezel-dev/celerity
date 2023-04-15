# Expression Statement

```ebnf
expression-statement ::= expression
```

Evaluates [`expression`](../expressions.md) (the *result*). If the expression
statement is the final statement of the enclosing
[block expression](../expressions/block-expression.md), *result* becomes the
result of the block expression.
