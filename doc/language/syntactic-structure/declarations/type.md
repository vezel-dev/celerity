# Type Declaration

```ebnf
type-declaration ::= ('pub' 'opaque'?)? 'type' lower-identifier type-parameter-list? '=' type ';'
type-parameter-list ::= '(' type-parameter (',' type-parameter)* ')'
type-parameter ::= attribute* type-parameter-binding
type-parameter-binding ::= lower-identifier |
                           discard-identifier
```
