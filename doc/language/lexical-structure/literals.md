# Literals

```ebnf
literal ::= nil-literal |
            boolean-literal |
            integer-literal |
            real-literal |
            atom-literal |
            string-literal
```

```ebnf
nil-literal ::= 'nil'
```

```ebnf
boolean-literal ::= 'true' |
                    'false'
```

```ebnf
integer-literal ::= binary-integer-literal |
                    octal-integer-literal |
                    decimal-integer-literal |
                    hexadecimal-integer-literal
binary-integer-literal ::= '0' [bB] binary-digit ('_'* binary-digit)*
binary-digit ::= [0-1]
octal-integer-literal ::= '0' [oO] octal-digit ('_'* octal-digit)*
octal-digit ::= [0-7]
decimal-integer-literal ::= decimal-digit ('_'* decimal-digit)*
decimal-digit ::= [0-9]
hexadecimal-integer-literal ::= '0' [xX] hexadecimal-digit ('_'* hexadecimal-digit)*
hexadecimal-digit ::= [0-9a-fA-F]
```

```ebnf
real-literal ::= real-part '.' real-part ([eE] [+-]? real-part)?
real-part ::= decimal-digit ('_'* decimal-digit)*
```

```ebnf
atom-literal ::= ':' (upper-identifier |
                      lower-identifier)
```

```ebnf
string-literal ::= '"' ([^#xa#xd#x85#x2028#x2029"\] |
                        string-escape-sequence)* '"'
string-escape-sequence ::= '\' (string-escape-code |
                                string-escape-unicode)
string-escape-code ::= [0nNrRtT"\]
string-escape-unicode ::= [uU] hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit
```
