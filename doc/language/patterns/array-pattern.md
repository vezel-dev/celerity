# Array Pattern

```ebnf
array-pattern ::= array-pattern-clause ('::' binding ('::' array-pattern-clause)?)? |
                  binding '::' array-pattern-clause
array-pattern-clause ::= '[' (pattern (',' pattern)* ','?)? ']'
```
