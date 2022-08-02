namespace Common.Models;

public record Star(RegexTree SubExpression) : RegexTree
{
    public override bool Empty => true;

    public override ISet<Letter> First { get; } = SubExpression.First;
    
    public override ISet<Letter> Next
    {
        set
        {
            NextBackingField = value;
            SubExpression.Next = SubExpression.First.Union(value).ToHashSet();
        }
    }

    public override ISet<Letter> Last { get; } = SubExpression.Last;
    public override IEnumerable<Letter> Letters { get; } = SubExpression.Letters;
    public override string RegexString { get; } = $"({SubExpression.RegexString}*)";
}
