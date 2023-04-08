# Literals

```ebnf
literal ::= nil-literal |
            boolean-literal |
            integer-literal |
            real-literal |
            atom-literal |
            string-literal
```

## Nil Literal

```ebnf
nil-literal ::= 'nil'
```

## Boolean Literal

```ebnf
boolean-literal ::= 'true' |
                    'false'
```

## Integer Literal

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

## Real Literal

```ebnf
real-literal ::= real-part '.' real-part ([eE] [+-]? real-part)?
real-part ::= decimal-digit ('_'* decimal-digit)*
```

## Atom Literal

```ebnf
atom-literal ::= ':' (upper-identifier |
                      lower-identifier)
```

## String Literal

```ebnf
string-literal ::= '"' ([^#xa#xd#x85#x2028#x2029"\] |
                        string-escape-sequence)* '"'
string-escape-sequence ::= '\' (string-escape-simple |
                                string-escape-unicode)
string-escape-simple ::= [0aAbBeEfFnNrRtTvV"\]
string-escape-unicode ::= [uU] hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit hexadecimal-digit
```

### Verbatim String Literal

```ebnf
verbatim-string-literal ::= '"""' '"'* [^#xa#xd#x85#x2028#x2029]+ '"""' '"'*
```

### Block String Literal

```ebnf
block-string-literal ::= '"""' '"'* white-space* line-break block-string-line white-space* '"""' '"'*
block-string-line ::= [^#xa#xd#x85#x2028#x2029]* line-break
```
