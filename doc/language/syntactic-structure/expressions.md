# Expressions

```ebnf
expression ::= assignment-expression
prefix-expression ::= unary-expression |
                      primary-expression |
                      postfix-expression
primary-expression ::= parenthesized-expression |
                       block-expression |
                       identifier-expression |
                       literal-expression |
                       module-expression |
                       record-expression |
                       error-expression |
                       tuple-expression |
                       array-expression |
                       set-expression |
                       map-expression |
                       lambda-expression |
                       if-expression |
                       condition-expression |
                       match-expression |
                       receive-expression |
                       while-expression |
                       for-expression |
                       return-expression |
                       raise-expression |
                       next-expression |
                       break-expression
postfix-expression ::= primary-expression (field-expression |
                                           index-expression |
                                           call-expression |
                                           send-expression)*
```
