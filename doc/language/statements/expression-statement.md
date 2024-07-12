# Expression Statement

```ebnf
expression-statement ::= (block-expression |
                          if-expression |
                          condition-expression |
                          match-expression |
                          receive-expression |
                          while-expression |
                          for-expression |
                          try-expression) ';'? | expression ';'
```

Evaluates [`expression`](../expressions.md) (the *value*). If the expression
statement is the final statement of the enclosing
[block expression](../expressions/block-expression.md), the *value* becomes the
result of the block expression.
