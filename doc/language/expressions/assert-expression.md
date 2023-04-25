# Assert Expression

```ebnf
assert-expression ::= 'assert' expression
```

Evaluates [`expression`](../expressions.md) (the *condition*). If the result
does not test as [truthy](../expressions/control/if-expression.md#truthiness), a
panic occurs. An `assert` expression is always executed; it is not conditional
on build flags or similar.

<!-- TODO: Link to panic definition. -->

An `assert` expression is typically used in a
[`test` declaration](../declarations/test-declaration.md) to verify the outcome
of a unit test, but it can also be used in regular code. The runtime system is
allowed to optimize with the assumption that *condition* holds after the
`assert` expression.

The result of an `assert` expression is the value of the *condition*.
