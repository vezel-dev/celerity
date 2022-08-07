# Error Expression

```ebnf
error-expression ::= 'err' upper-identifier '{' (error-expression-field (',' error-expression-field)* ','?)? '}'
error-expression-field ::= 'mut'? lower-identifier ':' expression
```
