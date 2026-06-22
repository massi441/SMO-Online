using SMOO.Util;

namespace SMOO.Serialization;

internal interface IDeserializableStruct
{
    void Deserialize(ref SpanReader reader);
}
