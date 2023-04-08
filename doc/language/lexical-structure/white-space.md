# White Space

```ebnf
blank ::= white-space |
          line-break
line-break ::= [#xa#xd#x85#x2028#x2029]
```

## White Space

```ebnf
white-space ::= [#x9#xb#xc#x20#xa0#x1680#x#x2000-#x2009#x200a#x202f#x205f#x3000]
```

## Line Break

```ebnf
line-break ::= [#xa#xd#x85#x2028#x2029]
```
