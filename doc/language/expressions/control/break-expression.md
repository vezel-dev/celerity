# Break Expression

```ebnf
break-expression ::= 'break' break-expression-result?
break-expression-result ::= 'as' expression
```

Stops the evaluation of the enclosing [`while` expression](while-expression.md)
or [`for` expression](for-expression.md). In other words, a `break` expression
is semantically equivalent to transferring control to just after the final
statement of the *loop body*, and then performing no further iterations. If a
`break-expression-result` is present, the [`expression`](../../expressions.md)
(the *result*) becomes the result of the enclosing `while` expression or `for`
expression.

Any [`defer` statements](../../statements/defer-statement.md) that would go out
of scope due to the control transfer are executed before stopping the loop.

It is a semantic error for a `break` expression to appear outside of the *loop
body* of a `while` expression or `for` expression. If a `break` expression
appears in the *body* of a `defer` statement, it cannot bind to a `while`
expression or `for` expression outside of the *body*.

A `break` expression has no result value.
