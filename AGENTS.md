# SMO-Online coding conventions

SMO-Online is a C# (.NET 10) UDP game server. Readability, Modularity, and Testability are the main goals. The code should be easy to read, easy to change, and easy to test.

## Project documentation

- [README.md](README.md): general project overview.
- [DESIGN.md](DESIGN.md): in depth explanation of the server architecture and packet flow.

## C# style

- Use explicit types. Only use `var` when the type is unreadable otherwise.
- Never use primary constructors.
- Prefer to declare objects as named, explicitly typed locals instead of passing `new X(...)` inline as an argument.
- Use collection expressions (`[a, b]`, `[]`), never `new[] { a, b }` or `new List<T> { ... }`.
- Always use braces, on their own lines, even for single-line `if` bodies.
- Do not write inline `//` comments that explain code. Use `///` XML doc summaries on types and non-trivial members instead.
- Everything in the `SMOO` project is `internal`.
- Expected failures return `ServerResult` / `ServerResult<T>` with a `ServerError`. Do not throw exceptions for control flow.
