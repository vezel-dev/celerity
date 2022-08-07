# Array Pattern

```ebnf
array-pattern ::= array-pattern-clause ('::' pattern-binding ('::' array-pattern-clause)?)? |
                  pattern-binding '::' array-pattern-clause
array-pattern-clause ::= '[' (pattern (',' pattern)* ','?)? ']'
```
