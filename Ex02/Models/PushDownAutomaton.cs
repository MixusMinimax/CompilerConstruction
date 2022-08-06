namespace Ex02.Models;

public record PushDownAutomaton(
    Grammar Grammar,
    IReadOnlyDictionary<Symbol, SymbolMetadata> Metadata,
    PushDownTable PushDownTable,
    LookaheadTable LookaheadTable);