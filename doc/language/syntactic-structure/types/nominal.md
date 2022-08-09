# Nominal Type

```ebnf
nominal-type ::= nominal-type-path? lower-identifier nominal-type-argument-list?
nominal-type-path ::= module-path '.'
nominal-type-argument-list ::= '(' (nominal-type-argument (',' nominal-type-argument)*)? ')'
nominal-type-argument ::= type
```
