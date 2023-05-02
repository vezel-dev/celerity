# Types

```ebnf
primary-type ::= nominal-type |
                 literal-type |
                 boolean-type |
                 integer-type |
                 real-type |
                 atom-type |
                 string-type |
                 reference-type |
                 handle-type |
                 module-type |
                 record-type |
                 error-type |
                 tuple-type |
                 array-type |
                 set-type |
                 map-type |
                 function-type |
                 agent-type
type ::= any-type |
         unknown-type |
         union-type |
         variable-type
type-annotation ::= ':' type
```

## Return Types

```ebnf
return-type ::= regular-return-type |
                none-return-type
regular-return-type ::= type
none-return-type ::= 'none'
return-type-annotation ::= '->' return-type return-type-annotation-raise?
return-type-annotation-raise ::= 'raise' type
```
