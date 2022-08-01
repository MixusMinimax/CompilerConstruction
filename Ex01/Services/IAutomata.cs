using Ex01.Exceptions;
using Ex01.Models;

namespace Ex01.Services;

public interface IAutomata
{
    public INode Run(IEnumerable<char> input);

    public bool Accepts(IEnumerable<char> input) => Accepts(input, out _);


    public bool Accepts(IEnumerable<char> input, out INode finalNode)
    {
        try
        {
            finalNode = Run(input);
            return finalNode.IsFinal;
        }
        catch (InvalidCharacterException e)
        {
            finalNode = e.Node;
            return false;
        }
    }
}