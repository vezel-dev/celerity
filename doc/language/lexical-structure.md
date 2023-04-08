# Lexical Structure

```ebnf
module-input ::= shebang-line? token*
interactive-input ::= token*
token ::= blank |
          comment |
          operator |
          punctuator |
          keyword |
          identifier |
          literal
```
