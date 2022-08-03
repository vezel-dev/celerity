# String Pattern

```ebnf
string-pattern ::= string-pattern-clause ("::" pattern-binding ("::" string-pattern-clause)?)? |
                   pattern-binding "::" string-pattern-clause
string-pattern-clause ::= string-literal
```
