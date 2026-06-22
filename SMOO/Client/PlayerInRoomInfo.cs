using SMOO.Attributes;
using SMOO.Server;
using SMOO.MemUtil;

namespace SMOO.Client;

internal readonly struct PlayerInRoomInfo
{
    [RequiredField]
    public readonly byte PlayerIndex;

    [DynamicField(MaxSize = Constants.MaxPlayerNameLength)]
    public readonly StreamStringView<byte> PlayerName;

    [DynamicField(MaxSize = Constants.MaxCostumeNameLength)]
    public readonly StreamStringView<byte> CostumeBody;

    [DynamicField(MaxSize = Constants.MaxCostumeNameLength)]
    public readonly StreamStringView<byte> CostumeCap;

    public PlayerInRoomInfo(Player player)
    {
        PlayerIndex = player.Slot;

        PlayerName = new StreamStringView<byte>(player.Name);
        CostumeBody = new StreamStringView<byte>(player.WorldInfo.CostumeBody);
        CostumeCap = new StreamStringView<byte>(player.WorldInfo.CostumeCap);
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.Write(PlayerIndex);

        PlayerName.Serialize(ref writer);
        CostumeBody.Serialize(ref writer);
        CostumeCap.Serialize(ref writer);
    }
}
