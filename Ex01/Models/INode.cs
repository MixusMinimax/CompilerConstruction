namespace Ex01.Models;

public interface INode
{
    public bool IsFinal { get; }
    public INode this[char character] { get; set; }
}