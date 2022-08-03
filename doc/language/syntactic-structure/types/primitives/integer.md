# Integer Type

```ebnf
integer-type ::= "int" integer-type-range?
integer-type-range ::= "(" (integer-type-range-bound ".." |
                            ".." integer-type-range-bound |
                            integer-type-range-bound ".." integer-type-range-bound) ")"
integer-type-range-bound ::= "-"? integer-literal
```
