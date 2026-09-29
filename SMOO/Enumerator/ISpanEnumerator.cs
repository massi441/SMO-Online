namespace SMOO.Enumerator;

// TODO: Move to Core

/// <summary>
/// A stack-only enumerator for iterating over a collection of items of type <typeparamref name="T"/>, providing a lightweight and efficient way to traverse collections without heap allocations
/// </summary>
/// <typeparam name="T">The type of the items in the collection</typeparam>
/// <typeparam name="TSelf">The type of the enumerator</typeparam>
internal interface ISpanEnumerator<T, TSelf> : IDisposable where TSelf : allows ref struct
{
    T Current { get; }
    TSelf GetEnumerator();
    bool MoveNext();
}
