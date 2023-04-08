# Identifiers

```ebnf
identifier ::= upper-identifier |
               lower-identifier |
               discard-identifier
```

## Uppercase Identifier

```ebnf
upper-identifier ::= [A-Z] [0-9a-zA-Z]*
```

## Lowercase Identifier

```ebnf
lower-identifier ::= [a-z] [_0-9a-z]*
```

## Discard Identifier

```ebnf
discard-identifier ::= '_' [_0-9a-z]*
```
