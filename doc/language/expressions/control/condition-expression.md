# Condition Expression

```ebnf
condition-expression ::= 'cond' '{' condition-expression-arm (',' condition-expression-arm)* ','? '}'
condition-expression-arm ::= expression '->' expression
```

For each `condition-expression-arm`, in lexical order, evaluates the first
[`expression`](../../expressions.md) (the *condition*). The first *condition*
that tests as [truthy](if-expression.md#truthiness) causes the associated second
`expression` (the *body*) to be evaluated and the evaluation of arms to stop. If
no *body* is evaluated during this process, a panic occurs.

<!-- TODO: Link to panic definition. -->

The whole expression results in the value of the evaluated *body*.
