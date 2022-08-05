# Record Expression

```ebnf
record-expression ::= 'rec' '{' (record-expression-field (',' record-expression-field)*)? '}'
record-expression-field ::= 'mut'? lower-identifier ':' expression
```
