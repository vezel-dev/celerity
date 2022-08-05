# Record Pattern

```ebnf
record-pattern ::= 'rec' '{' (record-pattern-field (',' record-pattern-field)*)? '}'
record-pattern-field ::= lower-identifier ':' pattern
```
