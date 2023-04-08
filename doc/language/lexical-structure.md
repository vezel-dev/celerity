# Lexical Structure

```ebnf
input ::= shebang-.ine? token*
token ::= blank |
          comment |
          operator |
          punctuator |
          keyword |
          identifier |
          literal
```
