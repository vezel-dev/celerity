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
pattern-binding ::= pattern-variable-binding |
                    pattern-discard-binding
pattern-variable-binding ::= 'mut'? lower-identifier
pattern-discard-binding ::= discard-identifier
pattern-alias ::= 'as' pattern-variable-binding
```
