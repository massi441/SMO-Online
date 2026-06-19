namespace SMOO.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class DynamicFieldAttribute : Attribute
{
    public required ushort MaxSize { get; init; }
}
