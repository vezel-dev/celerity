# Lambda Expression

```ebnf
lambda-expression ::= 'err'? 'fn' lambda-parameter-list '->' expression
lambda-parameter-list ::= '(' (lambda-parameter (',' lambda-parameter)* ','?)? ')'
lambda-parameter ::= attribute* lambda-parameter-binding
lambda-parameter-binding ::= binding-identifier
```
