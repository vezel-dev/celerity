# Type Declaration

```ebnf
type-declaration ::= ("pub" "opaque"?)? "type" value-identifier type-parameter-list? "=" type
type-parameter-list ::= "(" type-parameter ("," type-parameter)* ")"
type-parameter ::= attribute* value-identifier
```
