# Lexical Structure

```ebnf
module-input ::= shebang-line? token*
interactive-input ::= token*
token ::= white-space |
          comment |
          operator |
          punctuator |
          keyword |
          identifier |
          literal
```
