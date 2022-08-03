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
nil-literal ::= "nil"
```

```ebnf
boolean-literal ::= "true" |
                    "false"
```

```ebnf
integer-literal ::= decimal-digit ("_"? decimal-digit)* |
                    "0" [bB] binary-digit ("_"? binary-digit)* |
                    "0" [oO] octal-digit ("_"? octal-digit)* |
                    "0" [xX] hexadecimal-digit ("_"? hexadecimal-digit)* |
decimal-digit ::= [0-9]
binary-digit ::= [0-1]
octal-digit ::= [0-7]
hexadecimal-digit ::= [0-9a-fA-F]
```

```ebnf
real-literal ::= real-part "." real-part ([eE] [+-]? real-part)?
real-part ::= [0-9]+
```

```ebnf
atom-literal ::= ":" identifier
```

```ebnf
string-literal ::= '"' ([^"\] | string-escape-sequence) '"'
string-escape-sequence ::= "\" (string-escape-code | string-escape-unicode)
string-escape-code ::= [0nNrRtT"\]
string-escape-unicode ::= [uU] [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F]
```
