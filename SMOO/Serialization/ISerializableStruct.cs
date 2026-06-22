using SMOO.MemUtil;

namespace SMOO.Serialization;

internal interface ISerializableStruct
{
    void Serialize(ref SpanWriter writer);
}
