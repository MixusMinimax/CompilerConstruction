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

        if (_cache.Count is >= CacheMaxSize and > 0)
            _cache.Remove(_cache.MinBy(e => e.Value.LastUpdate).Key);
        return (_cache[grammar] = (DateTime.Now, new ReadOnlyDictionary<Symbol, SymbolMetadata>(metadata))).MetaData;
    }

    private bool ComputeIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata)
    {
        var isEmptyExisting = GetIsEmpty(symbol, metadata);
        if (isEmptyExisting is not null) return (bool)isEmptyExisting;

        return symbol switch
        {
            Epsilon => SetIsEmpty(symbol, metadata, true),
            Terminal => SetIsEmpty(symbol, metadata, false),
            // TODO: Get productions of non terminal, and if any of the RHSs can be empty, the non terminal can too.
            //       A RHS can be empty, if all symbols within can be empty.
            NonTerminal nonTerminal => throw new NotImplementedException(),
            _ => false
        };
    }

    private bool SetIsEmpty(Symbol symbol, Dictionary<Symbol, SymbolMetadata> metadata, bool isEmpty)
    {
        metadata[symbol] = (metadata.GetValueOrDefault(symbol) ?? new SymbolMetadata(symbol)) with
        {
            IsEmpty = isEmpty
        };

        return isEmpty;
    }

    private bool? GetIsEmpty(Symbol symbol, IReadOnlyDictionary<Symbol, SymbolMetadata> metadata)
    {
        return metadata.GetValueOrDefault(symbol)?.IsEmpty;
    }
}