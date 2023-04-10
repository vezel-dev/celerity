# While Expression

```ebnf
while-expression ::= 'while' expression block-expression while-expression-else?
while-expression-else ::= 'else' block-expression
```

Evaluates the first [`block-expression`](../block.md) (the *loop body*)
repeatedly until the [`expression`](../../expressions.md) (the *condition*) no
longer tests as [truthy](if.md#truthiness). The *condition* is evaluated at the
beginning of every iteration. If no iterations run and a
`while-expression-else` is present, the second `block-expression` (the *else
body*) is evaluated once.

The whole expression results in one of the following values, in order:

1. The result of any [`break as` expression](break.md) within the *loop body*.
2. The result of the *loop body*, if at least one iteration completes.
3. The result of the *else body*, if a `while-expression-else` is present.
4. The [nil value](../../../lexical-structure/literals.md#nil-literal).
