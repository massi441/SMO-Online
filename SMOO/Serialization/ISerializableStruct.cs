using SMOO.Util;

namespace SMOO.Serialization;

internal interface ISerializableStruct
{
    void Serialize(ref SpanWriter writer);
}
