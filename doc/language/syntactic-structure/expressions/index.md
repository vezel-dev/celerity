# Index Expression

```ebnf
index-expression ::= index-argument-list
index-argument-list ::= "[" (index-argument ("," index-argument)* ("," index-variadic-argument)?)? "]"
index-argument ::= expression
index-variadic-argument ::= ".." index-argument
```
