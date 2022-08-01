using Ex01.Models;

namespace Ex01.Services;

public abstract class Automata : IAutomata
{
    protected abstract INode GetStartNode();

    public INode Run(IEnumerable<char> input)
    {
        return input.Aggregate(GetStartNode(), (currentNode, inputChar) => currentNode[inputChar]);
    }
}