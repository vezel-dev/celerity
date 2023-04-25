# Record Expression

```ebnf
record-expression ::= 'rec' record-expression-with? '{' (record-expression-field (',' record-expression-field)* ','?)? '}'
record-expression-with ::= 'with' expression
record-expression-meta ::= 'meta' expression
record-expression-field ::= 'mut'? code-identifier ':' expression
```
