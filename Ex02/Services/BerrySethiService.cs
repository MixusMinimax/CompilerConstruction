using System.Collections;
using System.Collections.Immutable;
using Common.Extensions;
using Common.Models;
using CollectionExtensions = Common.Extensions.CollectionExtensions;

namespace Ex02.Services;

public interface IBerrySethiService
{
    public RegexTree ConstructExample();

    public Task ConvertToNfa(RegexTree regexTree, TextWriter writer);

    public Task<IDictionary<BitArray, IDictionary<char, BitArray>>> ConvertToDfa(RegexTree regexTree,
        StreamWriter resultWriter, TextWriter writer, string input,
        IDictionary<BitArray, IDictionary<char, BitArray>>? previousTransitions = default);
}

public class BerrySethiService : IBerrySethiService
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
                    from next in source.Key.GetNext()
                    select (Source: source.Value, Destination: states[next], Label: next.Character)).Concat(
                    from first in regexTree.First
                    select (Source: 0, Destination: states[first], Label: first.Character))
                select $"  {edge.Source} -> {edge.Destination} [label=\"{edge.Label}\"];\n") +
            "  start -> 0\n" +
            "}");

        await writer.FlushAsync();
    }

    private static BitArray GetBitArray(int size, int? value = default)
    {
        return new BitArray(size)
        {
            [value ?? 0] = value is not null
        };
    }

    public async Task<IDictionary<BitArray, IDictionary<char, BitArray>>> ConvertToDfa(
        RegexTree regexTree, StreamWriter resultWriter, TextWriter writer, string input,
        IDictionary<BitArray, IDictionary<char, BitArray>>? previousTransitions = default)
    {
        regexTree.GetNext();
        var letters = new[] { new Letter('?') { Next = regexTree.First } }.Concat(regexTree.Letters).ToList();
        var states = letters.Select((node, i) => (Index: i, Node: node))
            .ToImmutableDictionary(a => a.Node, b => b.Index, keyComparer: new IdentityEqualityComparer<RegexTree>());
        Letter GetLetter(int x) => letters[x];
        int GetIndex(RegexTree node) => states[node];
        var stateCount = letters.Count;

        var transitions = previousTransitions is not null
            ? new Dictionary<BitArray, IDictionary<char, BitArray>>(previousTransitions,
                new CollectionExtensions.BitArrayEqualityComparer())
            : new Dictionary<BitArray, IDictionary<char, BitArray>>(
                new CollectionExtensions.BitArrayEqualityComparer());

        BitArray GetNext(BitArray currentState, char character)
        {
            if (currentState.Cast<bool>().All(e => !e)) return GetBitArray(stateCount);

            var mapping =
                transitions.ComputeIfAbsent(currentState,
                    _ => new Dictionary<char, BitArray>());

            return mapping.ComputeIfAbsent(character, _ => currentState
                .GetIndices()
                .SelectMany(index => GetLetter(index).GetNext().Select(GetIndex))
                .Distinct()
                .Select(index => (Index: index, Letter: GetLetter(index)))
                .Where(element => element.Letter.Character == character)
                .Select(element => GetBitArray(stateCount, element.Index))
                .Aggregate(GetBitArray(stateCount), (acc, e) => acc.Or(e))
            );
        }

        var state = input.Aggregate(GetBitArray(stateCount, 0), GetNext);

        var finalStates = regexTree.Last
            .Select(GetIndex).Aggregate(GetBitArray(stateCount), (acc, e) => acc.Or(GetBitArray(stateCount, e)));
        if (regexTree.Empty) finalStates[0] = true;
        bool IsFinalState(BitArray s) => new BitArray(finalStates).And(s).Cast<bool>().Any(e => e);

        var allStates = transitions.Keys.Concat(transitions.Values.SelectMany(e => e.Values))
            .Distinct(new CollectionExtensions.BitArrayEqualityComparer()).ToList();

        // converts Bitarray with indices 1,2,3 to "n_1_2_3"
        string StateToDotId(BitArray s) => "n_" + string.Join('_', s.GetIndices());

        string StateToDotLabel(BitArray s) =>
            s.Cast<bool>().All(e => !e) ? "∅" : $"{{{string.Join(',', s.GetIndices())}}}";

        async Task GenerateDotState(BitArray s) =>
            await writer.WriteLineAsync($"  {StateToDotId(s)} [label=\"{StateToDotLabel(s)}\"];");

        async Task GenerateDotEdge(BitArray from, char character, BitArray to) =>
            await writer.WriteLineAsync(
                $"  {StateToDotId(from)} -> {StateToDotId(to)} [label=\"{character}\"];");

        await writer.WriteLineAsync("digraph dfa {\n" +
                                    "  rankdir=LR;\n" +
                                    "  size=\"8,5\"\n" +
                                    "  node [shape=none,width=0,height=0,margin=0]; start [label=\"\"];\n" +
                                    "  node [shape=doublecircle];");

        foreach (var s in allStates.Where(IsFinalState))
            await GenerateDotState(s);

        await writer.WriteLineAsync("  node [shape=circle];");

        foreach (var s in allStates.Where(e => !IsFinalState(e)))
            await GenerateDotState(s);

        foreach (var mapping in transitions)
        foreach (var (character, next) in mapping.Value)
            await GenerateDotEdge(mapping.Key, character, next);

        await writer.WriteLineAsync("  start -> n_0;\n" +
                                    "}");
        await writer.FlushAsync();

        await resultWriter.WriteLineAsync($"The word \"{input}\" is {(IsFinalState(state) ? "" : "not ")}accepted.");

        return transitions;
    }
}
// dfa -o ../../../dfa.gv bbabbabaaab
