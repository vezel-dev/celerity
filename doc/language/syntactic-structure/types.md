# Types

```ebnf
type ::= primary-type ('or' primary-type)*
return-type ::= none-type |
                type
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
```

```ebnf
type-annotation ::= ':' type
return-type-annotation ::= '->' return-type
```
