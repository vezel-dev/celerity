# Operators

```ebnf
operator ::= custom-operator |
             special-operator
```

```ebnf
custom-operator ::= custom-additive-operator |
                    custom-multiplicative-operator |
                    custom-bitwise-operator |
                    custom-shift-operator
custom-additive-operator ::= [+-~] operator-part*
custom-multiplicative-operator ::= [*/%] operator-part*
custom-bitwise-operator ::= [&|^] operator-part*
custom-shift-operator ::= [><] operator-part*
operator-part ::= [+-~*/%&|^><]
```

```ebnf
special-operator ::= "<-" |
                     "=" |
                     "==" |
                     "!=" |
                     ">" |
                     ">=" |
                     "<" |
                     "<="
```
