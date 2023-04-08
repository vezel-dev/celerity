# Types

```ebnf
primary-type ::= any-type |
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
                 agent-type |
                 nominal-type
type ::= primary-type ('or' primary-type)*
type-annotation ::= ':' type
```

## Return Types

```ebnf
return-type ::= none-type |
                type
return-type-annotation ::= '->' return-type return-type-annotation-raise?
return-type-annotation-raise ::= 'raise' type
```
