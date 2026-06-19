namespace SMOO.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class DynamicRepeatedFieldAttribute : Attribute
{
    public required Type Type { get; init; }
    public required int MaxRepeatCount { get; init; }
}
