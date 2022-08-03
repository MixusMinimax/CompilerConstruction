using System.Text.RegularExpressions;

namespace Ex02.Models;

public abstract record Symbol
{
    public abstract bool Empty { get; }
}

public record NonTerminal(string Name) : Symbol
{
    private bool _empty;

    public void SetEmpty(bool value) => _empty = value;

    public override bool Empty => _empty;

    public override string ToString() => Regex.IsMatch(Name, "[A-Z]+\\d+") ? Name : $"<{Name}>";
}

public record Terminal(string Name, bool Quoted = false) : Symbol
{
    public override bool Empty => false;

    public override string ToString() => Quoted ? $"\"{Name}\"" : Name;
}

public record Epsilon() : Terminal("()")
{
    public override bool Empty => true;
}
