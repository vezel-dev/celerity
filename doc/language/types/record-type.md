# Record Type

```ebnf
record-type ::= 'rec' '{' (record-type-field (',' record-type-field)* ','?)? '}'
record-type-field ::= 'mut'? code-identifier ':' type
```
