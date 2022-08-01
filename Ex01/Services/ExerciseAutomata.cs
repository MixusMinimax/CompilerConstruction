using Ex01.Models;

namespace Ex01.Services;

public class ExerciseAutomata : Automata
{
    private INode StartNode { get; init; }

    protected override INode GetStartNode() => StartNode;

    public ExerciseAutomata()
    {
        StartNode = new Node(0);
        var one = new Node(1);
        var two = new Node(2, true);
        var three = new Node(3);
        StartNode['a'] = one;
        StartNode['b'] = two;
        one['d'] = two;
        one['c'] = three;
        three['e'] = one;
        two['f'] = three;
    }
}