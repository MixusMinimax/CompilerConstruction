using System.Collections.Immutable;
using Common.Extensions;
using Ex02.Models;

namespace Ex02.Util;

public static class GrammarUtils
{
    public static IImmutableSet<Terminal> Concatenate(this IImmutableSet<Terminal> first,
        IImmutableSet<Terminal> second)
    {
        return first.Contains(Epsilon.Instance) ? first.Remove(Epsilon.Instance).Union(second) : first;
    }

    // Metadata modifications

    public static bool SetIsEmpty(
        this Dictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol, bool isEmpty)
    {
        metadata[symbol] = (metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol)) with
        {
            IsEmpty = isEmpty
        };

        return isEmpty;
    }

    public static bool? GetIsEmpty(
        this IReadOnlyDictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol)
    {
        return metadata.GetValueOrDefault(symbol)?.IsEmpty;
    }

    public static IImmutableSet<Terminal> SetFEps(
        this Dictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol,
        IImmutableSet<Terminal> fEps)
    {
        var result = metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol);

        metadata[symbol] = result with
        {
            FEps = fEps,
            First = result.IsEmpty ?? false ? fEps.Add(Epsilon.Instance) : fEps
        };

        return fEps;
    }

    public static IImmutableSet<Terminal>? GetFEps(
        this IReadOnlyDictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol)
    {
        return metadata.GetValueOrDefault(symbol)?.FEps;
    }

    public static IImmutableSet<Terminal> SetFollow(
        this Dictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol, IImmutableSet<Terminal> follow)
    {
        metadata[symbol] = (metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol)) with
        {
            Follow = follow
        };

        return follow;
    }

    public static IImmutableSet<Terminal>? GetFollow(
        this IReadOnlyDictionary<Symbol, SymbolMetadata> metadata,
        Symbol symbol)
    {
        return metadata.GetValueOrDefault(symbol)?.Follow;
    }


    public static IImmutableSet<Terminal> GetFirst(
        this IReadOnlyDictionary<Symbol, SymbolMetadata> metadata,
        IEnumerable<Symbol> symbols)
    {
        symbols = symbols.ToList();
        var fEps = symbols
            .TakeWhileIncluding(symbol => (bool)metadata.GetIsEmpty(symbol)!)
            .Select(symbol => metadata.GetFEps(symbol)!)
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, fEps) => set.Union(fEps));
        var isEmpty = symbols.All(symbol => (bool)metadata.GetIsEmpty(symbol)!);
        return isEmpty ? fEps.Add(Epsilon.Instance) : fEps;
    }
}