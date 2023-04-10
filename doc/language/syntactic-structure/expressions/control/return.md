# Return Expression

```ebnf
return-expression ::= 'tail'? 'ret' expression
```

Evaluates [`expression`](../../expressions.md) (the *result*), explicitly
returns *result* from the current
[`fn` declaration](../../declarations/function.md) or
[lambda expression](../values/lambda.md), and transfers control to the caller.

Any [`defer` statements](../../statements/defer.md) that would go out of scope
due to the control transfer are executed after evaluating *result*, but before
returning it to the caller.

It is a semantic error for a `ret` expression to appear outside of an `fn`
declaration or lambda expression. A `ret` expression cannot appear in the *body*
of a `defer` statement, unless it is nested in a lambda expression.

A `ret` expression has no result value.

<!-- TODO: Document tail calls. -->
