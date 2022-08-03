namespace Ex02.Models;

public record Production(NonTerminal From, RightHandSide[] RightHands)
{
    public override string ToString()
    {
        var from = From.ToString();
        var padding = new string(' ', from.Length);
        return
            $"{from} -> {string.Join<RightHandSide>($"\n{padding}  | ", RightHands)}";
    }
}

public record RightHandSide(params Symbol[] Symbols)
{
    public int Length => Symbols.Length == 1 && Symbols[0] is Epsilon
        ? 0
        : Symbols.Length > 0 && Symbols[^1] is EndOfInput
            ? Symbols.Length - 1
            : Symbols.Length;

    public override string ToString() => string.Join<Symbol>(' ', Symbols);
}
