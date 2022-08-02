namespace Ex02.Models;

public record Star(RegexTree SubExpression) : RegexTree
{
    public override bool Empty => true;

    public override ISet<Letter> First { get; } = SubExpression.First;

    private ISet<Letter>? _next;

    public override ISet<Letter> Next
    {
        get => _next ?? (Next = new HashSet<Letter>());
        set
        {
            _next = value;
            SubExpression.Next = SubExpression.First.Union(value).ToHashSet();
        }
    }

    public override ISet<Letter> Last { get; } = SubExpression.Last;
    public override IEnumerable<Letter> Letters { get; } = SubExpression.Letters;
    public override string RegexString { get; } = $"({SubExpression.RegexString}*)";
}
