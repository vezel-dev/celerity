# Any Type

```ebnf
any-type ::= 'any'
```

`any` is a special type that, like [`unk`](unknown-type.md), allows any value.
Unlike `unk`, however, `any` allows you to perform any operation on the value,
even if the type checker would otherwise be able to prove that a given operation
is incorrect.

The type of any operation on a value typed as `any` is also `any`. A value of
type `any` can be assigned to a location of any other type.

`unk` should generally be preferred over `any` wherever possible.
