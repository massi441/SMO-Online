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

## Architecture notes

- Each room processes its messages sequentially on a single task. Do not add locks inside room code.
- Players are disconnected through `IRoom.RequestDisconnection`. The disconnection happens after the current room message is processed, never in the middle of a handler.
- Sequenced (reliable) packets live in each player's `SequencedPacketStore`. Broadcasts share one `RentedBuffer`, and the receiver's sequence number is written into it right before every send and resend.
- `RentedBuffer` is reference counted: every holder calls `Acquire`/`Transfer` and releases its own reference.

## Tests

- xUnit v3 with NSubstitute. Test classes are `public` and mirror the `SMOO` folder layout.
- Name tests `ClassName_Behavior` or `ClassName_Behavior_Condition`.
- Every test has `// Arrange`, `// Act` and `// Assert` sections (Sometimes multiple of those sections or none at all).
- Use `StubFactory` for players, contexts and buffers, and `SMOTestUtil` for shared helpers. Use `NullLogger.Instance` unless the test checks logs.
- Theory parameters must be primitives or strings, since internal types cannot appear in a public test signature. Build internal objects inside the test.

## Git

- Commit messages should be short and self explanatory. e.g. "Remove self deletion from updater"
