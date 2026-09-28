namespace SMOO.Server;

// TODO: Inline in files

internal class Constants
{
    // Packet
    public const uint Magic = 0x534D4F4F; // "SMOO"
    public const byte Version = 1;

    // Data constraints
    public const byte MaxPlayerNameLength = 50;

    public const byte MaxCostumeNameLength = 64;
    public const byte MaxAnimNameLength = 64;
    public const byte MaxStageNameLength = 255;
    public const byte MaxBlendWeights = 6;
    public const ushort MaxChatMessageLength = 512;
    public const ushort MaxBufferSize = 2048;

    // Threading/Time
    public const int PlayerHealthCheckThreshold = 3000;
    public const int PlayerConnectionLostThreshold = 10000;

    // Server Config
}
