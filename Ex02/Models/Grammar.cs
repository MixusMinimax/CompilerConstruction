namespace Ex02.Models;

public record Grammar(
    Dictionary<string, NonTerminal> NonTerminals,
    Dictionary<string, Terminal> Terminals,
    Dictionary<string, Production> Productions,
    NonTerminal StartSymbol)
{
    public override string ToString()
    {
        return string.Join<Production>('\n', Productions.Values);
    }
}
