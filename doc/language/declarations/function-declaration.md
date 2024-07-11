# Function Declaration

```ebnf
function-declaration ::= 'pub'? (function-declaration-signature block-expression |
                                 'ext' function-declaration-signature ';')
function-declaration-signature ::= 'err'? 'fn' code-identifier function-parameter-list return-type-annotation?
function-parameter-list ::= '(' (function-parameter (',' function-parameter)* ','?)? ')'
function-parameter ::= attribute* function-parameter-binding type-annotation?
function-parameter-binding ::= binding-identifier
```
