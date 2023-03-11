# Function Type

```ebnf
function-type ::= 'fn' |
                  'err'? 'fn' function-type-signature?
function-type-signature ::= '(' function-type-parameter-list return-type-annotation ')'
function-type-parameter-list ::= '(' (function-type-parameter (',' function-type-parameter)*)? ')'
function-type-parameter ::= attribute* type
```
