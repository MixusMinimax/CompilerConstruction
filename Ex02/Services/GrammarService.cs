using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Common;
using Common.Extensions;
using Ex02.Models;
using Ex02.Util;

namespace Ex02.Services;

public interface IGrammarService
{
    public IReadOnlyDictionary<Symbol, SymbolMetadata> GetMetadata(Grammar grammar);
}

public class GrammarService : IGrammarService
{
    private const int CacheMaxSize = 256;

    private readonly ICache<Grammar, IReadOnlyDictionary<Symbol, SymbolMetadata>> _cache =
        new Cache<Grammar, IReadOnlyDictionary<Symbol, SymbolMetadata>>();

    public IReadOnlyDictionary<Symbol, SymbolMetadata> GetMetadata(Grammar grammar)
    {
        if (_cache.TryGetValue(grammar, out var result)) return result;

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
            ComputeFollow(symbol, metadata, grammar);
        }

        return _cache.Set(grammar, new ReadOnlyDictionary<Symbol, SymbolMetadata>(metadata));
    }

    // IsEmpty

    private static bool ComputeIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata, Grammar grammar,
        IImmutableSet<NonTerminal>? usedNonTerminals = default)
    {
        var isEmptyExisting = metadata.GetIsEmpty(symbol);
        if (isEmptyExisting is not null) return (bool)isEmptyExisting;

        return symbol switch
        {
            Epsilon => metadata.SetIsEmpty(symbol, true),
            Terminal => metadata.SetIsEmpty(symbol, false),
            NonTerminal nonTerminal => metadata.SetIsEmpty(symbol,
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
        var fEpsExisting = metadata.GetFEps(symbol);
        if (fEpsExisting is not null) return fEpsExisting;

        return symbol switch
        {
            Epsilon => metadata.SetFEps(symbol, ImmutableHashSet<Terminal>.Empty),
            Terminal terminal => metadata.SetFEps(symbol, Enumerable.Repeat(terminal, 1).ToImmutableHashSet()),
            NonTerminal nonTerminal => metadata.SetFEps(symbol,
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
                rightHand => rightHand.Symbols.TakeWhileIncluding(symbol => (bool)metadata.GetIsEmpty(symbol)!))
            .Select(symbol => ComputeFEps(symbol, metadata, grammar, usedNonTerminals))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, fEps) => set.Union(fEps));
    }

    // Follow

    private record struct ProductionOccurrence(NonTerminal From, RightHandSide RightHandSide, int Index);

    private static IImmutableSet<Terminal> ComputeFollow(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata,
        Grammar grammar, IImmutableSet<Symbol>? usedSymbols = default,
        Dictionary<Symbol, List<ProductionOccurrence>>? relevantProductionsCache =
            default)
    {
        var followExisting = metadata.GetFollow(symbol);
        if (followExisting is not null) return followExisting;
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
                .TakeWhileIncluding(sym => (bool)metadata.GetIsEmpty(sym)!)
                .Select(sym => metadata.GetFEps(sym)!))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, follow) => set.Union(follow));

        var fromParents = relevantProductions
            .Where(production => production.RightHandSide.Symbols
                .Skip(production.Index + 1)
                .All(sym => (bool)metadata.GetIsEmpty(sym)!))
            .Select(production => ComputeFollow(production.From, metadata, grammar, usedSymbols))
            .Aggregate(ImmutableHashSet<Terminal>.Empty, (set, follow) => set.Union(follow));

        return metadata.SetFollow(symbol, fromFollowing.Union(fromParents));
    }
}