using System.Text.RegularExpressions;

namespace Ex02.Models;

public abstract record Symbol;

public record NonTerminal(string Name) : Symbol
{
    public override string ToString() => Regex.IsMatch(Name, "[A-Z]+('|_?\\d+)") ? Name : $"<{Name}>";
}

public record Terminal(string Name, bool Quoted = false) : Symbol
{
    public override string ToString() => Quoted ? $"\"{Name}\"" : Name;
}

public record EndOfInput() : Terminal("$")
{
    public override string ToString() => base.ToString();
}

public record Epsilon() : Terminal("()")
{
    public override string ToString() => base.ToString();
}
