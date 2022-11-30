# Type Checking

### Lexer
The lexer does no type checking.

### Parser
The parser has static analysis on types. It will see if 'A != B' basically. I don't want the
language to have any inheritance or strange casts. So a forbidden example is you cannot return a string to implicitly
convert it into a list.

All symbols are stored in `_staticHints`. It is mapped as `Symbol -> Expected-Type`. Function identifiers
are saved too, as they hinted return type.

During an assignment, the hint must be specified. Then, if there's an identifier we can do the following.
`If identifier is not present within _staticHints, the symbol is undefined`
`If the corresponding TypeHint from _staticHints for identifier does not match the specified hint, the types do not match`

### Interpreter
The interpreter is assured that things will match because of the parser, but most cases must be handled
specifically.