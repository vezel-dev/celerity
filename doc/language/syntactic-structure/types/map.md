# Map Type

```ebnf
map-type ::= "mut"? "#" "[" map-type-pair ("," map-type-pair)* "]"
map-type-pair ::= type ":" "?"? type
```
