# Try Expression

```ebnf
try-expression ::= 'try' expression 'catch' '{' try-expression-arm (',' try-expression-arm)* ','? '}'
try-expression-arm ::= try-catch-pattern try-expression-arm-guard? '->' expression
try-expression-arm-guard ::= 'if' expression
```
