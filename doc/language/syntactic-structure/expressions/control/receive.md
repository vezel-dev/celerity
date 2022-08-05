# Receive Expression

```ebnf
receive-expression ::= 'recv' '{' receive-expression-arm+ '}' receive-expression-else?
receive-expression-arm ::= pattern receive-expression-arm-guard? '=>' expression ';'
receive-expression-arm-guard ::= 'if' expression
receive-expression-else ::= 'else' block-expression
```
