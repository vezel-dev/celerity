# Error Type

```ebnf
error-type ::= "err" module-identifier? "{" (error-type-field ("," error-type-field)*)? "}"
error-type-field ::= "mut"? value-identifier ":" type
```
