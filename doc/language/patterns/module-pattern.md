# Module Pattern

```ebnf
module-pattern ::= 'mod' module-path? '{' (module-pattern-field (',' module-pattern-field)* ','?)? '}'
module-pattern-field ::= code-identifier ':' pattern
```
