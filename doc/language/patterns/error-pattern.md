# Error Pattern

```ebnf
error-pattern ::= 'err' upper-identifier? '{' (error-pattern-field (',' error-pattern-field)* ','?)? '}'
error-pattern-field ::= code-identifier ':' pattern
```
