namespace Ex02.Models;

public record Token(Terminal Terminal, string Value)
{
    public override string ToString() => $"{Terminal}(\"{Value}\")";
}