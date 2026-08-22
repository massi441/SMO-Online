using System.Reflection;
using System.Runtime.InteropServices;
using SMOO.Attributes;

namespace SMOO.Memory;

// TODO: Fix boolean and char marshal size mismatch from C#

/// <summary>
/// A reflection helper class for computing the minimum and maxium byte size of a struct
/// </summary>
/// <typeparam name="T">The type of struct to measure the size of</typeparam>
internal static class RequiredSize<T> where T : struct, allows ref struct
{
    public static readonly ushort MinSize;
    public static readonly ushort MaxSize;

    static RequiredSize()
    {
        (MinSize, MaxSize) = Compute(typeof(T));
    }

    private static (ushort Min, ushort Max) Compute(Type type)
    {
        ushort minSize = 0;
        ushort maxSize = 0;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        bool hasSizeFields = fields.Any(f => f.IsDefined(typeof(RequiredFieldAttribute)) || f.IsDefined(typeof(DynamicFieldAttribute)) || f.IsDefined(typeof(DynamicRepeatedFieldAttribute)));
        if (!hasSizeFields)
        {
            minSize += (ushort)Marshal.SizeOf(type);
            return (minSize, minSize);
        }

        foreach (FieldInfo field in fields)
        {
            if (field.IsDefined(typeof(RequiredFieldAttribute)))
            {
                (ushort Min, ushort Max) = Compute(field.FieldType);
                minSize += Min;
                maxSize += Max;
            }
            else if (field.IsDefined(typeof(DynamicFieldAttribute)))
            {
                DynamicFieldAttribute attribute = field.GetCustomAttribute<DynamicFieldAttribute>()!;

                (ushort Min, ushort _) = Compute(field.FieldType);

                minSize += Min;
                maxSize += (ushort)(Min + attribute.MaxSize);
            }
            else if (field.IsDefined(typeof(DynamicRepeatedFieldAttribute)))
            {
                DynamicRepeatedFieldAttribute attribute = field.GetCustomAttribute<DynamicRepeatedFieldAttribute>()!;

                (ushort _, ushort MaxTypeSize) = Compute(attribute.Type);
                maxSize += (ushort)(MaxTypeSize * attribute.MaxRepeatCount);
            }
        }

        return (minSize, maxSize);
    }
}
