# Condition Expression

```ebnf
condition-expression ::= 'cond' '{' condition-expression-arm (',' condition-expression-arm)* ','? '}'
condition-expression-arm ::= expression '=>' expression
```
