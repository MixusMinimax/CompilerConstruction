namespace Common.Models;

public record Concat(RegexTree Left, RegexTree Right) : RegexTree
{
    public override bool Empty { get; } = Left.Empty && Right.Empty;

    public override ISet<Letter> First { get; } = Left.Empty ? Left.First.Union(Right.First).ToHashSet() : Left.First;

    public override ISet<Letter> Next
    {
        set
        {
            NextBackingField = value;
            Left.Next = Right.Empty ? Right.First.Union(value).ToHashSet() : Right.First.ToHashSet();
            Right.Next = value;
        }
    }

    public override ISet<Letter> Last { get; } = Right.Empty ? Left.Last.Union(Right.Last).ToHashSet() : Right.Last;

    public override IEnumerable<Letter> Letters { get; } = Left.Letters.Concat(Right.Letters).ToArray();

    public override string RegexString { get; } = $"{Left.RegexString}{Right.RegexString}";
}
