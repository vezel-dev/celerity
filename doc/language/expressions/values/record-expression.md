# Record Expression

```ebnf
record-expression ::= 'rec' record-expression-with? '{' (record-expression-field (',' record-expression-field)* ','?)? '}'
record-expression-with ::= 'with' expression
record-expression-field ::= 'mut'? lower-identifier ':' expression
```
