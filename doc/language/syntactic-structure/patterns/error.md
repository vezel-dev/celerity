# Error Pattern

```ebnf
error-pattern ::= "err" module-identifier? "{" (error-pattern-field ("," error-pattern-field)*)? "}"
error-pattern-field ::= value-identifier ":" pattern
```
