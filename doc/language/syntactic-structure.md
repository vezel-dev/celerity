# Syntactic Structure

```ebnf
module ::= attribute* 'mod' module-path ';' declaration*
```

```ebnf
module-path ::= upper-identifier ('::' upper-identifier)*
```
