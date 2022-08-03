# Lexical Structure

```ebnf
input ::= shebang-line? token*
token ::= white-space |
          comment |
          operator |
          punctuator |
          keyword |
          identifier |
          literal
```
