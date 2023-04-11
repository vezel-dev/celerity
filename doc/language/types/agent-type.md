# Agent Type

```ebnf
agent-type ::= 'agent' agent-type-protocol?
agent-type-protocol ::= '{' agent-type-message (',' agent-type-message)* ','? '}'
agent-type-message ::= code-identifier agent-type-message-parameter-list
agent-type-message-parameter-list ::= '(' (agent-type-message-parameter (',' agent-type-message-parameter)*)? ')'
agent-type-message-parameter ::= type
```
