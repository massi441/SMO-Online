namespace SMOO.Server;

internal class Constants
{
    // Packet
    public const uint Magic = 0x534D4F4F; // "SMOO"
    public const byte Version = 1;

    // Data constraints
    public const byte MaxRetries = 5;
    public const byte MaxPlayerNameLength = 50;

    public const byte MaxCostumeNameLength = 64;
    public const byte MaxAnimNameLength = 64;
    public const byte MaxStageNameLength = 255;
    public const byte MaxBlendWeights = 6;
    public const ushort MaxChatMessageLength = 512;
    public const ushort MaxBufferSize = 2048;

    // Room
    public const byte DefaultRoomSize = 4;
    public const byte MaxRoomSize = 10;

    // Threading/Time
    public const int ResendThreadTick = 1000;
    public const int DefaultResendDelay = 500;
    public const int PlayerHealthCheckThreshold = 3000;
    public const int PlayerHealthCheckTick = 1500;
    public const int PlayerConnectionLostThreshold = 10000;
    public const int PlayerSynAckDelay = 10000;

    // Player
    public static readonly string DefaultCostumeName = "Mario";
}
