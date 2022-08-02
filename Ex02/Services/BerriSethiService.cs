using System.Collections.Immutable;
using System.Reflection.Emit;
using Ex02.Models;

namespace Ex02.Services;

public interface IBerriSethiService
{
    public RegexTree ConstructExample();

    public Task ConvertToNfa(RegexTree regexTree, TextWriter writer);
}

public class BerriSethiService : IBerriSethiService
{
    public RegexTree ConstructExample()
    {
        return new Concat(new Star(new Or(new Letter('a'), new Letter('b'))),
            new Concat(new Letter('a'), new Or(new Letter('a'), new Letter('b'))));
    }

    public async Task ConvertToNfa(RegexTree regexTree, TextWriter writer)
    {
        // Berry-Sethi algorithm
        // final states is the last states of root, including root if root can be empty.
        var letters = regexTree.Letters.ToList();
        var states = new[] { regexTree }.Concat(letters).Select((node, i) => (Index: i, Node: node))
            .ToImmutableDictionary(a => a.Node, b => b.Index, keyComparer: new IdentityEqualityComparer<RegexTree>());
        var finalStates = (regexTree.Empty ? new[] { regexTree } : Array.Empty<RegexTree>()).Concat(regexTree.Last)
            .ToList();

        await writer.WriteLineAsync(
            "digraph nfa {\n" +
            "  rankdir=LR;\n" +
            "  size=\"8,5\"\n" +
            "  node [shape=none,width=0,height=0,margin=0]; start [label=\"\"];\n" +
            "  node [shape=doublecircle];\n" +
            "  " + string.Concat(finalStates.Select(e => $"{states[e]};")) + "\n" +
            "  node [shape=circle];\n" + string.Concat(
                from edge in (from source in states
                    from next in ((RegexTree)source.Key).Next
                    select (Source: source.Value, Destination: states[next], Label: next.Character)).Concat(
                    from first in regexTree.First
                    select (Source: 0, Destination: states[first], Label: first.Character))
                select $"  {edge.Source} -> {edge.Destination} [label=\"{edge.Label}\"];\n") +
            "  start -> 0\n" +
            "}\n");

        await writer.FlushAsync();
    }
}
