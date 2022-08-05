# Lambda Expression

```ebnf
lambda-expression ::= 'fn' lambda-parameter-list 'err'? return-type-annotation? '=>' expression
lambda-parameter-list ::= '(' (lambda-parameter (',' lambda-parameter)* (',' lambda-variadic-parameter)?)? ')'
lambda-parameter ::= attribute* lambda-parameter-binding type-annotation?
lambda-parameter-binding ::= lower-identifier |
                             discard-identifier
lambda-variadic-parameter ::= '..' lambda-parameter
```
