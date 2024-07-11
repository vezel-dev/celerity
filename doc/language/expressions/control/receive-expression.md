# Receive Expression

```ebnf
receive-expression ::= 'recv' '{' receive-expression-arm (',' receive-expression-arm)* ','? '}' receive-expression-else?
receive-expression-arm ::= code-identifier receive-parameter-list receive-expression-arm-guard? '->' expression ','
receive-expression-arm-guard ::= 'if' expression
receive-expression-else ::= 'else' block-expression
receive-parameter-list ::= '(' (receive-parameter (',' receive-parameter)* ','?)? ')'
receive-parameter ::= pattern
```
