using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// A stack -only enumerator for iterating over <see cref="Player"/> instances, providing a lightweight and efficient way to traverse player collections without heap allocations
/// </summary>
/// <typeparam name="TSelf">The type of the enumerator, allowing generic functions to accepts specific player enumerators without needing to box the enumerator</typeparam>
internal interface IPlayerEnumerator<TSelf> : ISpanEnumerator<Player, TSelf> where TSelf : allows ref struct;
