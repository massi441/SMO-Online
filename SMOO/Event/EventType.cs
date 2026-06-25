namespace SMOO.Event;

internal enum EventType : ushort
{
    ChangeStage,
    ChangeCostume,
    ChangeCap,
    PlayerSync,

    /// <summary>
    /// A reserved EventType for server side validation
    /// </summary>
    OutOfRange
}
