namespace Ex02.Models;

public record State(NonTerminal NonTerminal, RightHandSide RightHandSide, int BulletPosition)
{
    public bool IsAtEnd => BulletPosition == RightHandSide.Length;

    public State MoveNext => BulletPosition < RightHandSide.Length
        ? this with { BulletPosition = BulletPosition + 1 }
        : throw new InvalidOperationException("Cannot advance past the end of the RHS");

    public bool IsBeforeNonTerminal =>
        BulletPosition < RightHandSide.Length && RightHandSide.Symbols[BulletPosition] is NonTerminal;

    public bool IsBeforeTerminal =>
        BulletPosition < RightHandSide.Length && RightHandSide.Symbols[BulletPosition] is Terminal;

    public bool IsBeforeEndOfInput =>
        BulletPosition < RightHandSide.Length && RightHandSide.Symbols[BulletPosition] is EndOfInput;

    // Write bullet between Symbols in RightHandSide at the current position
    public override string ToString()
    {
        var symbolsBeforeBullet = RightHandSide.Symbols.Take(BulletPosition);
        var symbolsAfterBullet = RightHandSide.Symbols.Skip(BulletPosition);
        return $"[{NonTerminal} -> {string.Join(" ", symbolsBeforeBullet)} * {string.Join(" ", symbolsAfterBullet)}]";
    }
}
