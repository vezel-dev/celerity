# Array Pattern

```ebnf
array-pattern ::= '[' ']' |
                  array-pattern-clause '..'? |
                  '..' array-pattern-clause
array-pattern-clause ::= '[' pattern (',' pattern)* ','? ']'
```
