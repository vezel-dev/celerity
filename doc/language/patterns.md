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

## Pattern Aliases

```ebnf
pattern-alias ::= 'as' pattern-variable-binding
```

## Pattern Bindings

```ebnf
binding ::= variable-binding |
            discard-binding
```

### Variable Bindings

```ebnf
variable-binding ::= 'mut'? lower-identifier
```

### Discard Bindings

```ebnf
discard-binding ::= discard-identifier
```
