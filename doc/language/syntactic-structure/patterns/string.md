# String Pattern

```ebnf
string-pattern ::= string-pattern-clause ('::' binding ('::' string-pattern-clause)?)? |
                   binding '::' string-pattern-clause
string-pattern-clause ::= string-literal
```
