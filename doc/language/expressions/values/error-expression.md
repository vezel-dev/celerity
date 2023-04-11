# Error Expression

```ebnf
error-expression ::= 'err' upper-identifier error-expression-with? '{' (error-expression-field (',' error-expression-field)* ','?)? '}'
error-expression-with ::= 'with' expression
error-expression-field ::= 'mut'? lower-identifier ':' expression
```
