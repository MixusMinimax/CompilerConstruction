namespace Ex02.Models;

public record Production(NonTerminal From, IEnumerable<IEnumerable<Symbol>> RightHands)
{
    public override string ToString()
    {
        var from = From.ToString();
        var padding = new string(' ', from.Length);
        return
            $"{from} -> {string.Join($"\n{padding}  | ", RightHands.Select(x => string.Join(" ", x)))}";
    }
}
