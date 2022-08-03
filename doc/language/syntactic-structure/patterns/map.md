# Map Pattern

```ebnf
map-pattern ::= "#" "[" (map-pattern-pair ("," map-pattern-pair)*)? "]"
map-pattern-pair ::= expression ":" pattern
```
