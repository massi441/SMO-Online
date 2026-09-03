# SMOO

A custom UDP server for Super Mario Odyssey online. It manages rooms of players and relays game state between them.

Focuses on **high performance** for a smooth experience. 

Written in **C#** with **.NET 10**.

## Build & Run

```sh
dotnet build
dotnet run --project SMOO
```

The server listens on **UDP** port **5001**, but can be configured with a `Config.json` file:
```json
{
  // this file is automatically created where the server is ran, the first time it is ran
  "port": 5001,
  "logLevel": "Trace"
}
```

## Code Design

- **Dependencies:** [`ServerContext`](SMOO/Server/ServerContext.cs) stores all services required by the server (packet controller, room manager, logger, etc...). Dependency injection is manual.
- **Memory:** ArrayPool buffers wrapped into [`SharedBuffer`](SMOO/Memory/SharedBuffer.cs)'s for automatic cleanup and atomic reference counting.
- **Serialization:** [`SpanReader`](SMOO/Memory/SpanReader.cs)/[`SpanWriter`](SMOO/Memory/SpanWriter.cs) for safe and fast reading/writing to raw memory streams.
- **Size bounds:** [`RequiredSize<T>`](SMOO/Memory/RequiredSize.cs) for deriving the minimum and maximum sizes of buffers to rent from the ArrayPool.
- **GC reduction:** [`Ref struct Enumerators`](SMOO/Enumerator) for 0 alloc enumerations of players in hot paths.
- **Threading:** Each room processes its messages sequentially, on its own processing loop. Room state is only ever mutated from that loop, meaning no thread synchronization is needed anywhere inside a room. Messages are uploaded to the room periodically by the [`Room Message Scheduler`](SMOO/Services/Impl/RoomMessageScheduler.cs).

More on packet flow [Here](DESIGN.md)

## References

N/A
