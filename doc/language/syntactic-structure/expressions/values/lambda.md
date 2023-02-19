# Lambda Expression

```ebnf
lambda-expression ::= 'fn' lambda-parameter-list '->' expression
lambda-parameter-list ::= '(' (lambda-parameter (',' lambda-parameter)*)? ')'
lambda-parameter ::= attribute* lambda-parameter-binding
lambda-parameter-binding ::= lower-identifier |
                             discard-identifier
```
