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

## Client

The game client is a **C++** mod that connects to this server, but currently isn't publicly available.

## Code Design

- **Dependencies:** [`ServerContext`](SMOO/Server/ServerContext.cs) stores all services required by the server (packet controller, room manager, logger, etc...). Dependency injection is manual.
- **Memory:** ArrayPool buffers wrapped into [`SharedBuffer`](SMOO/Memory/SharedBuffer.cs)'s for automatic cleanup and atomic reference counting.
- **Serialization:** [`SpanReader`](SMOO/Memory/SpanReader.cs)/[`SpanWriter`](SMOO/Memory/SpanWriter.cs) for safe and fast reading/writing to raw memory streams.
- **Size bounds:** [`RequiredSize<T>`](SMOO/Memory/RequiredSize.cs) for deriving the minimum and maximum sizes of buffers to rent from the ArrayPool.
- **GC reduction:** [`Ref struct Enumerators`](SMOO/Enumerator) for 0 alloc enumerations of players in hot paths.
- **Threading:** Each room processes its messages sequentially, on its own processing loop. Room state is only ever mutated from that loop, meaning no thread synchronization is needed anywhere inside a room. Messages are uploaded to the room periodically by the [`Room Message Scheduler`](SMOO/Services/Impl/RoomMessageScheduler.cs).

## Server & Packet Flow

The server holds a set of rooms, managed by the [`Room Holder`](SMOO/Services/Impl/RoomHolder.cs). Each room has its own set of players between which packets are relayed.

---

Before joining a room, a client must perform a **connection handshake** with the server. It follows the traditional model of TCP: 

- The client sends a SYN packet to the server, including the ID of the room they want to join. 
- The server responds to the client with a SYN-ACK packet, if the SYN packet is accepted.
- The client sends an ACK packet to complete the handshake. 

More details about packet reliability below.

---

Each SMOO packet is made up of a [`Packet Header`](SMOO/Protocol/PacketHeader.cs), and in most cases a payload. Each packet goes roughly through this flow:

**1. Arrival:** The packet reaches the [`Server`](SMOO/Server/UdpServer.cs) which waits with a UDP socket in a receive loop. When a packet arrives, it is copied into a buffer rented from the ArrayPool 
and wrapped into a [`SharedBuffer`](SMOO/Memory/SharedBuffer.cs).

**2. Validation & Routing:** Before being processed, the header of the packet is validated by the server (Magic Number, Packet Type, Room Id...). If validation passes, it is uploaded to a [`Room`](SMOO/Server/Room.cs)'s messaging queue
as a **packet** [`Room Message`](SMOO/Server/RoomMessage.cs). If validation fails, the packet is dropped.

**3. Processing:** Each room has its own processing loop, which waits forever until a new room message arrives, or if the room is shutdown. When a message arrives, it dispatched to its corresponding [`Message Processor`](SMOO/Services/Interface/IRoomMessageProcessor.cs).

When a **packet** message arrives, it is routed to the [`Packet Processor`](SMOO/Services/Impl/PacketMessageProcessor.cs). This processor performs extra validation on the header and the payload of the Packet, before dispatching the packet to its appropriate handler through the [`Packet Handler Table`](SMOO/Handle/PacketHandlerTable.cs). Each entry in that table maps directly to a handler for a given 
[`Packet Type`](SMOO/Protocol/PacketType.cs), allowing for direct indexing from a packet type.

> **Note:** Game packets are wrapped into Event packets. They are dispatched with the same mechanism as network packets by the [`Packet Event Handler`](SMOO/Handle/PacketEventHandler.cs)

**4. Response:** Most handlers end by relaying to other players in the room. Responses can be either fire-and-forget, or reliable, and sent via the [`Packet Controller`](SMOO/Services/Impl/PacketController.cs).

<dd>

**Fire-and-forget** is used for packets that don't need to reliably reach other players in the room, such as game synchronization packets: Player positions, animations, etc...

**Reliable** is used for packets that must reach other players in the room, as they contain state that cannot afford to be lost: Level changes, chat messages, costume changes, etc... Reliable
packets are stored in the [`Reliable Packet Store`](SMOO/Services/Impl/ReliablePacketStore.cs). They are resent by the [`Packet Resender`](SMOO/Services/Impl/PacketResendMessageProcessor.cs) at a fixed time interval, until they are acknowledged by the receiver. 
If a receiver fails to acknowledge a reliable packet after a certain period of time, they are disconnected from ther server.

</dd>

**5. Release:** Once the handler returns, the room releases its reference from the SharedBuffer. If the counter reaches 0, the buffer is returned to the ArrayPool. Reliable responses take their own reference for each receiver. 
The buffer stays alive until every receiver has acknowledged the packet, or until the packet expires for each receiver, as multiple reliable packets can share the same buffer in certain scenarios.

## References

N/A
