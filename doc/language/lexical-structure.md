# Lexical Structure

```ebnf
input ::= shebang-line? token*
token ::= blank |
          comment |
          operator |
          punctuator |
          keyword |
          identifier |
          literal
```
