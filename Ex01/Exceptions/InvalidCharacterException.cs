using Ex01.Models;

namespace Ex01.Exceptions;

public class InvalidCharacterException : Exception
{
    public INode Node { get; }

    public InvalidCharacterException(char character, INode node) : base($"Invalid character: {character}")
    {
        Node = node;
    }
}