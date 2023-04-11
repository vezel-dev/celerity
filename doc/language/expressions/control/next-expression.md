# Next Expression

```ebnf
next-expression ::= 'next'
```

Skips the remaining portion of the *loop body* of the enclosing
[`while` expression](while-expression.md) or
[`for` expression](for-expression.md) and starts a new iteration. In other
words, a `next` expression is semantically equivalent to transferring control to
just after the final statement of the *loop body*.

Any [`defer` statements](../../statements/defer-statement.md) that would go out
of scope due to the control transfer are executed before the next loop iteration
is started.

It is a semantic error for a `next` expression to appear outside of the *loop
body* of a `while` expression or `for` expression. If a `next` expression
appears in the *body* of a `defer` statement, it cannot bind to a `while`
expression or `for` expression outside of the *body*.

A `next` expression has no result value.
