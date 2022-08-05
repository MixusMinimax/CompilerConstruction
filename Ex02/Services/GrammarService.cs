using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Common.Extensions;
using Ex02.Models;

namespace Ex02.Services;

public interface IGrammarService
{
    public IReadOnlyDictionary<Symbol, SymbolMetadata> GetMetadata(Grammar grammar);
}

public class GrammarService : IGrammarService
{
    private const int CacheMaxSize = 256;

    private readonly Dictionary<Grammar, (
        DateTime LastUpdate,
        IReadOnlyDictionary<Symbol, SymbolMetadata> MetaData
        )> _cache = new();

    public IReadOnlyDictionary<Symbol, SymbolMetadata> GetMetadata(Grammar grammar)
    {
        if (_cache.TryGetValue(grammar, out var result))
        {
            _cache[grammar] = (DateTime.Now, result.MetaData);
            return result.MetaData;
        }

        var metadata = new Dictionary<Symbol, SymbolMetadata>();

        foreach (var symbol in grammar.Terminals.Values.Cast<Symbol>().Concat(grammar.NonTerminals.Values))
        {
            ComputeIsEmpty(symbol, metadata, grammar);
        }

        foreach (var symbol in grammar.Terminals.Values.Cast<Symbol>().Concat(grammar.NonTerminals.Values))
        {
            ComputeFEps(symbol, metadata, grammar);
        }

        foreach (var symbol in grammar.Terminals.Values.Cast<Symbol>().Concat(grammar.NonTerminals.Values))
        {
            ComputeFFollow(symbol, metadata, grammar);
        }

        if (_cache.Count is >= CacheMaxSize and > 0)
            _cache.Remove(_cache.MinBy(e => e.Value.LastUpdate).Key);
        return (_cache[grammar] = (DateTime.Now, new ReadOnlyDictionary<Symbol, SymbolMetadata>(metadata))).MetaData;
    }

    // IsEmpty

    private static bool ComputeIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata, Grammar grammar,
        IImmutableSet<NonTerminal>? usedNonTerminals = default)
    {
        var isEmptyExisting = GetIsEmpty(symbol, metadata);
        if (isEmptyExisting is not null) return (bool)isEmptyExisting;

        return symbol switch
        {
            Epsilon => SetIsEmpty(symbol, metadata, true),
            Terminal => SetIsEmpty(symbol, metadata, false),
            NonTerminal nonTerminal => SetIsEmpty(symbol, metadata,
                ComputeNonTerminalIsEmpty(nonTerminal, metadata, grammar, usedNonTerminals)),
            _ => false
        };
    }

    private static bool ComputeNonTerminalIsEmpty(NonTerminal nonTerminal, Dictionary<Symbol, SymbolMetadata> metadata,
        Grammar grammar, IImmutableSet<NonTerminal>? usedNonTerminals = default)
    {
        if (usedNonTerminals?.Contains(nonTerminal) ?? false) return false;

        usedNonTerminals ??= ImmutableHashSet<NonTerminal>.Empty;
        usedNonTerminals = usedNonTerminals.Add(nonTerminal);

        var production = grammar.Productions.GetValueOrDefault(nonTerminal.Name);
        if (production is null) return false;

        return production.RightHands.Any(rightHand =>
            rightHand.Symbols.All(symbol => ComputeIsEmpty(symbol, metadata, grammar, usedNonTerminals)));
    }

    // F_{\epsilon}

    private static IImmutableSet<Terminal> ComputeFEps(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata,
        Grammar grammar, IImmutableSet<NonTerminal>? usedNonTerminals = default)
    {
        var fEpsExisting = GetFEps(symbol, metadata);
        if (fEpsExisting is not null) return fEpsExisting;

        return symbol switch
        {
            Epsilon => SetFEps(symbol, metadata, ImmutableHashSet<Terminal>.Empty),
            Terminal terminal => SetFEps(symbol, metadata, Enumerable.Repeat(terminal, 1).ToImmutableHashSet()),
            NonTerminal nonTerminal => SetFEps(symbol, metadata,
                ComputeNonTerminalFEps(nonTerminal, metadata, grammar, usedNonTerminals)),
            _ => ImmutableHashSet<Terminal>.Empty
        };
    }

    private static IImmutableSet<Terminal> ComputeNonTerminalFEps(
        NonTerminal nonTerminal,
        Dictionary<Symbol, SymbolMetadata> metadata,
        Grammar grammar,
        IImmutableSet<NonTerminal>? usedNonTerminals)
    {
        if (usedNonTerminals?.Contains(nonTerminal) ?? false) return ImmutableHashSet<Terminal>.Empty;

        usedNonTerminals ??= ImmutableHashSet<NonTerminal>.Empty;
        usedNonTerminals = usedNonTerminals.Add(nonTerminal);

        var production = grammar.Productions.GetValueOrDefault(nonTerminal.Name);
        if (production is null) return ImmutableHashSet<Terminal>.Empty;

        return production.RightHands
            .SelectMany(
                rightHand => rightHand.Symbols.TakeWhileIncluding(symbol => (bool)GetIsEmpty(symbol, metadata)!))
            .Select(symbol => ComputeFEps(symbol, metadata, grammar, usedNonTerminals))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, fEps) => set.Union(fEps));
    }

    private record struct ProductionOccurrence(NonTerminal From, RightHandSide RightHandSide, int Index);

    private static IImmutableSet<Terminal> ComputeFFollow(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata,
        Grammar grammar, IImmutableSet<Symbol>? usedSymbols = default,
        Dictionary<Symbol, List<ProductionOccurrence>>? relevantProductionsCache =
            default)
    {
        var fFollowExisting = GetFFollow(symbol, metadata);
        if (fFollowExisting is not null) return fFollowExisting;
        if (usedSymbols?.Contains(symbol) ?? false) return ImmutableHashSet<Terminal>.Empty;

        usedSymbols ??= ImmutableHashSet<Symbol>.Empty;
        usedSymbols = usedSymbols.Add(symbol);

        relevantProductionsCache ??= new Dictionary<Symbol, List<ProductionOccurrence>>();

        var relevantProductions = relevantProductionsCache.ComputeIfAbsent(symbol, _ =>
        {
            // Get all RightHandSides that the symbol participates in.
            return grammar.Productions.Values
                .SelectMany(production =>
                    production.RightHands.Select(rhs => (production.From, RightHandSide: rhs)))
                .SelectWhere(tuple => tuple.RightHandSide.Symbols.Contains(symbol, out var index)
                    ? (ProductionOccurrence?)new ProductionOccurrence(tuple.From, tuple.RightHandSide, index)
                    : null)
                .ToList();
        });

        // Combine FEps from all RHSs that can be empty, including one more after that.

        var fromFollowing = relevantProductions
            .SelectMany(production => production.RightHandSide.Symbols.Skip(production.Index + 1)
                .TakeWhileIncluding(sym => (bool)GetIsEmpty(sym, metadata)!)
                .Select(sym => GetFEps(sym, metadata)!))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, fFollow) => set.Union(fFollow));

        // get all productions where everything right of the symbol can be empty,
        // and combine the FFollow of the left hand sides.
        // TODO

        var fromParents = relevantProductions
            .Where(production => production.RightHandSide.Symbols
                .Skip(production.Index + 1)
                .All(sym => (bool)GetIsEmpty(sym, metadata)!))
            .Select(production => ComputeFFollow(production.From, metadata, grammar, usedSymbols))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, fFollow) => set.Union(fFollow));

        return SetFFollow(symbol, metadata, fromFollowing.Union(fromParents));
    }

    // Metadata modifications

    private static bool SetIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata, bool isEmpty)
    {
        metadata[symbol] = (metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol)) with
        {
            IsEmpty = isEmpty
        };

        return isEmpty;
    }

    private static bool? GetIsEmpty(Symbol symbol, IReadOnlyDictionary<Symbol, SymbolMetadata> metadata)
    {
        return metadata.GetValueOrDefault(symbol)?.IsEmpty;
    }

    private static IImmutableSet<Terminal> SetFEps(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata,
        IImmutableSet<Terminal> fEps)
    {
        var result = metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol);

        metadata[symbol] = result with
        {
            FEps = fEps,
            FFirst = result.IsEmpty ?? false ? fEps.Add(Epsilon.Instance) : fEps
        };

        return fEps;
    }

    private static IImmutableSet<Terminal>? GetFEps(Symbol symbol, IReadOnlyDictionary<Symbol, SymbolMetadata> metadata)
    {
        return metadata.GetValueOrDefault(symbol)?.FEps;
    }

    private static IImmutableSet<Terminal> SetFFollow(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata,
        IImmutableSet<Terminal> fFollow)
    {
        metadata[symbol] = (metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol)) with
        {
            FFollow = fFollow
        };

        return fFollow;
    }

    private static IImmutableSet<Terminal>? GetFFollow(Symbol symbol,
        IReadOnlyDictionary<Symbol, SymbolMetadata> metadata)
    {
        return metadata.GetValueOrDefault(symbol)?.FFollow;
    }
}