# Identifiers

```ebnf
identifier ::= upper-identifier |
               lower-identifier |
               discard-identifier
```

```ebnf
upper-identifier ::= [A-Z] [0-9a-zA-Z]*
```

```ebnf
lower-identifier ::= [a-z] [_0-9a-z]*
```

```ebnf
discard-identifier ::= '_' [_0-9a-z]*
```
