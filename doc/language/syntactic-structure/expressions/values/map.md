# Map Expression

```ebnf
map-expression ::= 'mut'? '#' '[' (map-expression-pair (',' map-expression-pair)*)? ']'
map-expression-pair ::= expression ':' expression
```
