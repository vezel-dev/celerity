# Identifiers

```ebnf
identifier ::= module-identifier |
               value-identifier |
               discard-identifier
```

```ebnf
module-identifier ::= [A-Z] [a-zA-Z0-9]*
```

```ebnf
value-identifier ::= [a-z] [_a-z0-9]*
```

```ebnf
discard-identifier ::= "_" [_a-z0-9]*
```
