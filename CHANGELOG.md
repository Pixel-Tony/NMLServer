
## 1.x

### v1.0.4
- Added support for `badgetable` blocks and related variables
- Added diagnostics for `cargotable` blocks
- Fixed last value in comma-separated list being disregarded
- Fixed incorrect updates to unexpected tokens list during AST rebuild
- Fixed wrong paren being matched in `produce` block parser

### v1.0.3
- Fixed CR whitespace in builtin symbols after splitting grammar files on Windows

### v1.0.2
- Fixed incorrect argument passed to FindWhereOffset in AbstractSyntaxTree

### v1.0.1
- Fixed false error highlight for template identifiers.

### v1.0
- Initial public release.

## 0.x

### v0.2
- Add: completion suggestions for positions outside of tokens;
- Fix: corner case for TokenStorage.Rebuild if old count is 0.

### v0.1
- Initial public release for beta-testing.