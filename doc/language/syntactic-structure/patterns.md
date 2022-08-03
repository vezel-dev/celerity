# Patterns

```ebnf
pattern ::= (wildcard-pattern |
             literal-pattern |
             string-pattern |
             record-pattern |
             error-pattern |
             tuple-pattern |
             array-pattern |
             map-pattern |
             set-pattern |
             module-pattern) pattern-alias?
try-catch-pattern ::= (wildcard-pattern |
                       error-pattern) pattern-alias?
```

```ebnf
pattern-binding ::= "mut"? value-identifier |
                    discard-identifier
pattern-alias ::= "as" "mut"? value-identifier
```
