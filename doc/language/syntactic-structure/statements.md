# Statements

```ebnf
statement ::= attribute* (let-statement |
                          use-statement |
                          defer-statement |
                          assert-statement |
                          expression-statement) ';'
```

```ebnf
interactive-statement ::= attribute* (let-statement |
                                      expression-statement)
```
