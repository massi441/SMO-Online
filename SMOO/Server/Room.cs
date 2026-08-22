using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Memory;
using SMOO.Protocol;
using SMOO.Services.Interface;

namespace SMOO.Server;

/// <summary>
/// Represents a room of players in SMOO, from which messages can be relayed.
/// </summary>
internal class Room
{
    private readonly ServerContext _context;
    private Task _processTask = null!;
    private readonly Channel<RoomMessage> _messages;
    private readonly IRoomMessageProcessorList _serviceList;
    private readonly IRoomMessageScheduler _messageScheduler;
    
    public ushort Id { get; }
    public IPlayerHolder PlayerHolder { get; }
    public IBroadcaster Broadcaster { get; }
    public PlayerList Players => PlayerHolder.Players;

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IBroadcaster broadcaster, IRoomMessageProcessorList serviceList, IRoomMessageScheduler messageScheduler)
    {
        _context = conxtext;
        _messages = Channel.CreateUnbounded<RoomMessage>();
        _serviceList = serviceList;
        _messageScheduler = messageScheduler;

        Id = roomId;
        PlayerHolder = playerHolder;
        Broadcaster = broadcaster;
    }

    /// <summary>
    /// Starts the room's processing task and message scheduler
    /// </summary>
    public void Start()
    {
        _processTask = Task.Run(ProcessMessages, _context.CancellationToken);
        _messageScheduler.Start(this);
    }

    /// <summary>
    /// Uploads a message to the current room
    /// </summary>
    /// <param name="roomMessage">The message to upload</param>
    /// <exception cref="Exception">If the room fails to write the message to its message queue</exception>
    public void UploadMessage(RoomMessage roomMessage)
    {
#if DEBUG
        if (!_messages.Writer.TryWrite(roomMessage))
        {
            throw new Exception($"Writer failed to write room message in room #{Id}");
        }
#else
        _messages.Writer.TryWrite(roomMessage);
#endif
    }

    /// <summary>
    /// Uploads a message to the room with an empty packet.
    /// Note: The message processor tied to the message type must ignore the packet in the message
    /// </summary>
    /// <param name="type">The type of the message</param>
    public void UploadMessage(RoomMessageType type)
    {
        UploadMessage(new RoomMessage()
        {
            Type = type,
            Packet = null
        });
    }

    /// <summary>
    /// Processes the current messages in the room asychronously by dispatching
    /// them to the appropriate message processor
    /// </summary>
    /// <returns>A task processing the messages in the room</returns>
    private async Task ProcessMessages()
    {
        await foreach (RoomMessage message in _messages.Reader.ReadAllAsync())
        {
            IRoomMessageProcessor service = _serviceList.GetProcessor(message.Type);

            Packet? packet = message.Packet;
            if (packet != null)
            {
                using SharedBuffer buffer = packet.Value.Buffer;
                service.Process(this, packet.Value);
            }
            else
            {
                service.Process(this, default(Packet));
            }
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
    }

    /// <summary>
    /// Shuts down the room by stopping the processing loop and waiting on the message scheduler to be done
    /// </summary>
    /// <returns>A task to await for a full shutdown of the room</returns>
    public async Task Shutdown()
    {
        _context.Logger.LogInformation("Shutting down room #{RoomId}", Id);

        await _messageScheduler.Shutdown(); // need to wait as the scheduler uploads stuff to the writer

        _messages.Writer.Complete();

        await _processTask;
    }
}
