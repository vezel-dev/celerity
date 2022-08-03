# Function Declaration

```ebnf
function-declaration ::= "pub"? ("fn" value-identifier function-parameter-list block-expression |
                                 "ext" "fn" value-identifier function-parameter-list) "err"? return-type-annotation?
function-parameter-list ::= "(" (function-parameter ("," function-parameter)* ("," function-variadic-parameter)?)? ")"
function-parameter ::= attribute* (value-identifier |
                                   discard-identifier) type-annotation?
function-variadic-parameter ::= ".." function-parameter
```
