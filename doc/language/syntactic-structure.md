# Syntactic Structure

```ebnf
document ::= attribute* 'mod' module-path ';' declaration*
interactive ::= (interactive-declaration |
                 interactive-statement)*
```

```ebnf
module-path ::= upper-identifier ('::' upper-identifier)*
```
