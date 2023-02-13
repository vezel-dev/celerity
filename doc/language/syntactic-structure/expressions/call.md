# Call Expression

```ebnf
call-expression ::= call-argument-list call-try?
call-argument-list ::= '(' (call-argument (',' call-argument)*)? ')'
call-argument ::= expression
```

```ebnf
call-expression-try ::= '?' call-expression-try-catch?
call-expression-try-catch ::= 'catch' '{' call-expression-try-catch-arm+ '}'
call-expression-try-catch-arm ::= try-catch-pattern call-expression-try-catch-arm-guard? '=>' expression ';'
call-expression-try-catch-arm-guard ::= 'if' expression
```
