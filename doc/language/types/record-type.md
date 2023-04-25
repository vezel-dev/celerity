# Record Type

```ebnf
record-type ::= 'rec' '{' (record-type-field (',' record-type-field)* ','?)? '}' record-type-meta?
record-type-meta ::= 'meta' type
record-type-field ::= 'mut'? code-identifier ':' type
```
