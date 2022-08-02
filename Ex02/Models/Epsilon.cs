using System.Collections;

namespace Ex02.Models;

public record Epsilon : RegexTree
{
    public override bool Empty => true;

    public override ISet<Letter> First { get; } = new HashSet<Letter>();

    private ISet<Letter>? _next;

    public override ISet<Letter> Next
    {
        get => _next ?? (Next = new HashSet<Letter>());
        set => _next = value;
    }

    public override ISet<Letter> Last { get; } = new HashSet<Letter>();

    public override IEnumerable<Letter> Letters { get; } = Array.Empty<Letter>();

    public override string RegexString => "()";
}
