using System.Collections.ObjectModel;
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

        foreach (var symbol in grammar.NonTerminals.Values.Cast<Symbol>().Concat(grammar.Terminals.Values))
        {
            ComputeIsEmpty(symbol, metadata, grammar);
        }

        if (_cache.Count is >= CacheMaxSize and > 0)
            _cache.Remove(_cache.MinBy(e => e.Value.LastUpdate).Key);
        return (_cache[grammar] = (DateTime.Now, new ReadOnlyDictionary<Symbol, SymbolMetadata>(metadata))).MetaData;
    }

    private static bool ComputeIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata, Grammar grammar,
        ISet<NonTerminal>? usedNonTerminals = default)
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
        Grammar grammar, ISet<NonTerminal>? usedNonTerminals = default)
    {
        if (usedNonTerminals?.Contains(nonTerminal) ?? false) return false;

        usedNonTerminals = usedNonTerminals is not null ? usedNonTerminals.ToHashSet() : new HashSet<NonTerminal>();
        usedNonTerminals.Add(nonTerminal);

        var production = grammar.Productions.GetValueOrDefault(nonTerminal.Name);
        if (production is null) return false;

        return production.RightHands.Any(rightHand =>
            rightHand.Symbols.All(symbol => ComputeIsEmpty(symbol, metadata, grammar, usedNonTerminals)));
    }

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
}