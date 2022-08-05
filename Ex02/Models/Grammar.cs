using System.Collections.ObjectModel;
using Common.Extensions;

namespace Ex02.Models;

public record Grammar(
    ReadOnlyDictionary<string, NonTerminal> NonTerminals,
    ReadOnlyDictionary<string, Terminal> Terminals,
    ReadOnlyDictionary<string, Production> Productions,
    NonTerminal StartSymbol)
{
    public virtual bool Equals(Grammar? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return NonTerminals.EntriesEqual(other.NonTerminals) && Terminals.EntriesEqual(other.Terminals) &&
               Productions.EntriesEqual(other.Productions) && StartSymbol.Equals(other.StartSymbol);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NonTerminals, Terminals, Productions, StartSymbol);
    }

    public override string ToString()
    {
        return string.Join('\n', Productions.Values);
    }
}