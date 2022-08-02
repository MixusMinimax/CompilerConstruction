using System.Runtime.CompilerServices;

namespace Ex02;

public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public int GetHashCode(T value)
    {
        return RuntimeHelpers.GetHashCode(value);
    }

    public bool Equals(T? left, T? right)
    {
        return ReferenceEquals(left, right); // Reference identity comparison
    }
}