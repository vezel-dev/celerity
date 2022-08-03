# Expressions

```ebnf
expression ::= send-expression
prefix-expression ::= unary-expression |
                      primary-expression |
                      postfix-expression
primary-expression ::= parenthesized-expression |
                       block-expression |
                       identifier-expression |
                       literal-expression |
                       record-expression |
                       error-expression |
                       tuple-expression |
                       array-expression |
                       set-expression |
                       map-expression |
                       module-expression |
                       lambda-expression |
                       if-expression |
                       while-expression |
                       for-expression |
                       next-expression |
                       break-expression |
                       condition-expression |
                       match-expression |
                       return-expression |
                       raise-expression |
                       receive-expression
postfix-expression ::= primary-expression (field-expression |
                                           index-expression |
                                           call-expression |
                                           method-call-expression)*
```
