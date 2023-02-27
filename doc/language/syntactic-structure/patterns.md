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
pattern-alias ::= 'as' pattern-variable-binding
```

```ebnf
binding ::= variable-binding |
            discard-binding
variable-binding ::= 'mut'? lower-identifier
discard-binding ::= discard-identifier
```
