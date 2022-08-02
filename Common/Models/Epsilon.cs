namespace Common.Models;

public record Epsilon : RegexTree
{
    public override bool Empty => true;

    public override ISet<Letter> First { get; } = new HashSet<Letter>();

    public override ISet<Letter> Next
    {
        set => NextBackingField = value;
    }

    public override ISet<Letter> Last { get; } = new HashSet<Letter>();

    public override IEnumerable<Letter> Letters { get; } = Array.Empty<Letter>();

    public override string RegexString => "()";
}
