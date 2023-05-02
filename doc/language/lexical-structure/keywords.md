# Keywords

```ebnf
keyword ::= regular-keyword |
            type-keyword |
            reserved-keyword
```

## Regular Keywords

```ebnf
regular-keyword ::= 'and' |
                    'as' |
                    'assert' |
                    'break' |
                    'catch' |
                    'cond' |
                    'const' |
                    'defer' |
                    'else' |
                    'err' |
                    'ext' |
                    'false' |
                    'fn' |
                    'for' |
                    'if' |
                    'in' |
                    'let' |
                    'match' |
                    'meta' |
                    'mod' |
                    'mut' |
                    'next' |
                    'nil' |
                    'not' |
                    'opaque' |
                    'or' |
                    'pub' |
                    'raise' |
                    'rec' |
                    'recv' |
                    'ret' |
                    'tail' |
                    'test' |
                    'this' |
                    'true' |
                    'try' |
                    'type' |
                    'use' |
                    'while' |
                    'with'
```

## Type Keywords

```ebnf
type-keyword ::= 'agent' |
                 'any' |
                 'atom' |
                 'bool' |
                 'handle' |
                 'int' |
                 'none' |
                 'real' |
                 'ref' |
                 'str' |
                 'unk'
```

## Reserved Keywords

```ebnf
reserved-keyword ::= 'friend' |
                     'macro' |
                     'quote' |
                     'unquote' |
                     'yield'
```
