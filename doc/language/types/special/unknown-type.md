# Unknown Type

```ebnf
unknown-type ::= 'unk'
```

`unk` is a special type that, like [`any`](any-type.md), allows any value.
Unlike `any`, however, `unk` only allows a very limited set of operations which
are safe on all values. In other words, `unk` is the type-safe counterpart to
`any`.

A value of type `unk` can only be assigned to a location of type `unk` or `any`.

<!-- TODO: Describe semantics when checking an unk value's type dynamically. -->
