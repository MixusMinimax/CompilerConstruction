using System.Collections;

namespace Ex02.Models;

public abstract record RegexTree
{
    public abstract bool Empty { get; }
    public abstract ISet<Letter> First { get; }
    public abstract ISet<Letter> Next { get; set; }
    public abstract ISet<Letter> Last { get; }
    public abstract IEnumerable<Letter> Letters { get; }

    public abstract string RegexString { get; }
}
