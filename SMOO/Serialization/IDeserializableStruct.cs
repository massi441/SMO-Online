using SMOO.Memory;

namespace SMOO.Serialization;

internal interface IDeserializableStruct
{
    void Deserialize(ref SpanReader reader);
}
