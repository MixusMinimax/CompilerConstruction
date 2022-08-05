namespace Ex02.Models;

public record Production(NonTerminal From, RightHandSide[] RightHands)
{
    public virtual bool Equals(Production? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return From.Equals(other.From) && RightHands.SequenceEqual(other.RightHands);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, RightHands);
    }

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
    public virtual bool Equals(RightHandSide? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || Symbols.SequenceEqual(other.Symbols);
    }

    public override int GetHashCode()
    {
        return Symbols.GetHashCode();
    }

    public int Length => Symbols.Length == 1 && Symbols[0] is Epsilon
        ? 0
        : Symbols.Length > 0 && Symbols[^1] is EndOfInput
            ? Symbols.Length - 1
            : Symbols.Length;

    public override string ToString() => string.Join<Symbol>(' ', Symbols);
}