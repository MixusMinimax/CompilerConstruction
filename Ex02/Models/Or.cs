namespace Ex02.Models;

public record Or(RegexTree Left, RegexTree Right) : RegexTree
{
    public override bool Empty { get; } = Left.Empty || Right.Empty;

    public override ISet<Letter> First { get; } = Left.First.Union(Right.First).ToHashSet();

    private ISet<Letter>? _next;

    public override ISet<Letter> Next
    {
        get => _next ?? (Next = new HashSet<Letter>());
        set
        {
            _next = value;
            Left.Next = value;
            Right.Next = value;
        }
    }


    public override ISet<Letter> Last { get; } = Left.Last.Union(Right.Last).ToHashSet();

    public override IEnumerable<Letter> Letters { get; } = Left.Letters.Concat(Right.Letters).ToArray();

    public override string RegexString { get; } = $"({Left.RegexString}|{Right.RegexString})";
}
