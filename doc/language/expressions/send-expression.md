# Send Expression

```ebnf
send-expression ::= '->' lower-identifier send-argument-list
send-argument-list ::= '(' (send-argument (',' send-argument)*)? ')'
send-argument ::= expression
```
