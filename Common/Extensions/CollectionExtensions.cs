using System.Collections;

namespace Common.Extensions;

public static class CollectionExtensions
{
    public static TValue ComputeIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> factory) =>
        dict.TryGetValue(key, out var result) ? result : dict[key] = factory(key);

    public static IEnumerable<int> GetIndices(this BitArray bitArray) =>
        Enumerable.Range(0, bitArray.Count).Where(i => bitArray[i]);

    public static bool ElementsEqual(this BitArray input1, BitArray input2)
    {
        if (input1.Length != input2.Length)
            return false;

        var result = new BitArray(input1);
        result = result.Xor(input2);

        return !result.Cast<bool>().Contains(true);
    }

    public class BitArrayEqualityComparer : IEqualityComparer<BitArray>
    {
        bool IEqualityComparer<BitArray>.Equals(BitArray? x, BitArray? y) =>
            y != null && x != null && x.ElementsEqual(y);

        int IEqualityComparer<BitArray>.GetHashCode(BitArray obj) =>
            obj.GetIndices().Aggregate(0L, (acc, e) => acc | (uint)(1 << e)).GetHashCode();
    }

    public static bool EntriesEqual<TKey, TValue>(this IDictionary<TKey, TValue>? self,
        IDictionary<TKey, TValue>? other)
    {
        if (ReferenceEquals(self, other)) return true;
        if (self is null || other is null) return false;
        return self.Keys.Count == other.Keys.Count && self.All(kv => Equals(other[kv.Key], kv.Value));
    }

    public static IEnumerable<T> TakeWhileIncluding<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        foreach (var el in list)
        {
            yield return el;
            if (!predicate(el))
                yield break;
        }
    }

    public static IEnumerable<TReturn> SelectWhere<T, TReturn>(this IEnumerable<T> list,
        Func<T, TReturn?> selector)
        where TReturn : struct
    {
        return list.Select(selector).Where(result => result is not null).Select(result => result!.Value);
    }

    public static IEnumerable<TReturn> SelectWhere<T, TReturn>(this IEnumerable<T> list,
        Func<T, TReturn?> selector)
        where TReturn : class
    {
        return list.Select(selector).Where(result => result is not null).Select(result => result!);
    }

    public static bool Contains<T>(this IEnumerable<T> enumerable, T value, out int index)
    {
        index = 0;

        foreach (var el in enumerable)
        {
            if (Equals(el, value))
                return true;
            index++;
        }

        return false;
    }
}