# Assertion Statement

```ebnf
assert-statement ::= 'assert' expression
```

Evaluates [`expression`](../expressions.md) (the *condition*). If the result
does not test as [truthy](../expressions/control/if.md#truthiness), a panic
occurs. An `assert` statement is always executed; it is not conditional on build
flags or similar.

<!-- TODO: Link to panic definition. -->

An `assert` statement is typically used in a
[`test` declaration](../declarations/test.md) to verify the outcome of a unit
test, but it can also be used in regular code. The runtime system is allowed to
optimize with the assumption that *condition* holds after the `assert`
statement.
