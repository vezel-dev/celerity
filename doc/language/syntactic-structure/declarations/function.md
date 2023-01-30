# Function Declaration

```ebnf
function-declaration ::= 'pub'? ('fn' lower-identifier function-parameter-list 'err'? return-type-annotation? block-expression |
                                 'ext' 'fn' lower-identifier function-parameter-list 'err'? return-type-annotation?)
function-parameter-list ::= '(' (function-parameter (',' function-parameter)*)? ')'
function-parameter ::= attribute* function-parameter-binding type-annotation?
function-parameter-binding ::= lower-identifier |
                               discard-identifier
```
