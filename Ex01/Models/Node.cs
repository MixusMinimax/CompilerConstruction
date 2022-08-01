using Ex01.Exceptions;

namespace Ex01.Models;

public class Node : INode
{
    private IDictionary<char, INode> Edges { get; init; }
    private int Index { get; init; }

    public Node(int index, bool isFinal = false)
    {
        Index = index;
        IsFinal = isFinal;
        Edges = new Dictionary<char, INode>();
    }

    public bool IsFinal { get; }

    public INode this[char character]
    {
        get => Edges.TryGetValue(character, out var result) ? result : throw new InvalidCharacterException(character, this);
        set => Edges[character] = value;
    }

    public override string ToString()
    {
        return $"Node({Index}, isFinal = {IsFinal})";
    }
}