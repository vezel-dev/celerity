# Call Expression

```ebnf
call-expression ::= call-argument-list call-try?
method-call-expression ::= '->' lower-identifier call-argument-list call-try?
call-argument-list ::= '(' (call-argument (',' call-argument)* (',' call-variadic-argument)?)? ')'
call-argument ::= expression
call-variadic-argument ::= '..' call-argument
```

```ebnf
call-try ::= '?' call-try-catch?
call-try-catch ::= 'catch' '{' call-try-catch-arm+ '}'
call-try-catch-arm ::= try-catch-pattern call-try-catch-arm-guard? '=>' expression ';'
call-try-catch-arm-guard ::= 'if' expression
```
