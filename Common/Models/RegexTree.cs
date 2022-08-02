namespace Common.Models;

public abstract record RegexTree
{
    public abstract bool Empty { get; }
    public abstract ISet<Letter> First { get; }
    public abstract ISet<Letter> Next { set; }

    protected ISet<Letter>? NextBackingField;

    public IEnumerable<Letter> GetNext()
    {
        if (NextBackingField is null)
        {
            Next = new HashSet<Letter>();
        }

        return NextBackingField!;
    }

    public abstract ISet<Letter> Last { get; }
    public abstract IEnumerable<Letter> Letters { get; }

    public abstract string RegexString { get; }
}
