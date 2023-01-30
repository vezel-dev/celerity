# Function Type

```ebnf
function-type ::= 'fn' function-type-parameter-list 'err'? return-type-annotation
function-type-parameter-list ::= '(' (function-type-parameter (',' function-type-parameter)*)? ')'
function-type-parameter ::= attribute* type
```
