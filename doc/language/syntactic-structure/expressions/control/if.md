# If Expression

```ebnf
if-expression ::= 'if' expression block-expression if-expression-else?
if-expression-else ::= 'else' block-expression
```

Evaluates the [`expression`](../../expressions.md) (the *condition*). If the
result tests as [truthy](#truthiness), the first
[`block-expression`](../block.md) (the *then body*) is evaluated. Otherwise, if
an `if-expression-else` clause is present, the second `block-expression` (the
*else body*) is evaluated.

The whole expression results in one of the following values, in order:

1. The result of the *then body*.
2. The result of the *else body*, if an `if-expression-else` is present.
3. The [nil value](../../../lexical-structure/literals.md#nil-literal).

## Truthiness

<!-- TODO: Move this section elsewhere? -->

Almost all values are considered truthy, with the following exceptions:

* The [nil value](../../../lexical-structure/literals.md#nil-literal).
* The
  [`false` Boolean value](../../../lexical-structure/literals.md#boolean-literal).
