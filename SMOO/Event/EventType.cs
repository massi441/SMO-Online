namespace SMOO.Event;

internal enum EventType : ushort
{
    ChangeStage,
    ChangeCostume,
    ChangeCap,
    GameSync,

    /// <summary>
    /// A reserved EventType for server side validation
    /// </summary>
    OutOfRange
}
