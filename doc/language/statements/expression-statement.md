# Expression Statement

```ebnf
expression-statement ::= expression
```

Evaluates [`expression`](../expressions.md) (the *value*). If the expression
statement is the final statement of the enclosing
[block expression](../expressions/block-expression.md), the *value* becomes the
result of the block expression.
