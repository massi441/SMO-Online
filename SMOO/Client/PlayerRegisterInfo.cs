using System.Net;
using SMOO.Services.Interface;

namespace SMOO.Client;

/// <summary>
/// A DTO for making a player registration request to a Player Holder
/// </summary>
internal class PlayerRegisterInfo
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required IRoom Room { get; init; }
}
