namespace Ex02.Models;

public record Grammar(
    IEnumerable<NonTerminal> NonTerminals,
    IEnumerable<Terminal> Terminals,
    IEnumerable<Production> Productions)
{
    public override string ToString()
    {
        return string.Join('\n', Productions);
    }
}
