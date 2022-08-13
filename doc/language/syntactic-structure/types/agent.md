# Agent Type

```ebnf
agent-type ::= 'agent' '{' (agent-type-message (',' agent-type-message)* ','?)? '}'
agent-type-message ::= lower-identifier agent-type-message-parameter-list
agent-type-message-parameter-list ::= '(' (agent-type-message-parameter (',' agent-type-message-parameter)*)? ')'
agent-type-message-parameter ::= type
```
