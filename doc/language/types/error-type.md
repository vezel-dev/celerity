# Error Type

```ebnf
error-type ::= 'err' upper-identifier? '{' (error-type-field (',' error-type-field)* ','?)? '}'
error-type-field ::= 'mut'? code-identifier ':' type
```
