# Lambda Expression

```ebnf
lambda-expression ::= "fn" lambda-parameter-list "err"? return-type-annotation? "=>" expression
lambda-parameter-list ::= "(" (lambda-parameter ("," lambda-parameter)* ("," lambda-variadic-parameter)?)? ")"
lambda-parameter ::= attribute* (value-identifier |
                                 discard-identifier) type-annotation?
lambda-variadic-parameter ::= ".." lambda-parameter
```
