# Send Expression

```ebnf
send-expression ::= '->' code-identifier send-argument-list
send-argument-list ::= '(' (send-argument (',' send-argument)*)? ')'
send-argument ::= expression
```
