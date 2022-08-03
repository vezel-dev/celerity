# Error Expression

```ebnf
error-expression ::= "err" module-identifier "{" (error-expression-field ("," error-expression-field)*)? "}"
error-expression-field ::= "mut"? value-identifier ":" expression
```
