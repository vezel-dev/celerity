# Match Expression

```ebnf
match-expression ::= 'match' expression '{' match-expression-arm (',' match-expression-arm)* ','? '}'
match-expression-arm ::= pattern match-expression-arm-guard? '->' expression
match-expression-arm-guard ::= 'if' expression
```
