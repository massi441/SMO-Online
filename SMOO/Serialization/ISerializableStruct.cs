using SMOO.Memory;

namespace SMOO.Serialization;

internal interface ISerializableStruct
{
    void Serialize(ref SpanWriter writer);
}
