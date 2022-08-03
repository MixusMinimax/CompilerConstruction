using Ex02.Models;

namespace Ex02.Repositories;

public interface IGrammarRepository
{
    public Grammar GetRegexGrammar();

    public Grammar GetExerciseGrammar();
}

public class GrammarRepository : IGrammarRepository
{
    public Grammar GetRegexGrammar()
    {
        var nonTerminals = new Dictionary<string, NonTerminal>
        {
            ["S'"] = new("S'"),
            ["regex"] = new("regex"),
            ["A1"] = new("A1"),
            ["concat"] = new("concat"),
            ["A2"] = new("A2"),
            ["rep"] = new("rep"),
            ["A3"] = new("A3"),
            ["atom"] = new("atom")
        };
        var terminals = new Dictionary<string, Terminal>
        {
            ["$"] = new EndOfInput(),
            ["letter"] = new("letter"),
            ["|"] = new("|", true),
            ["*"] = new("*", true),
            ["("] = new("(", true),
            [")"] = new(")", true),
            ["e"] = new Epsilon(),
        };
        var productions = new Dictionary<string, Production>
        {
            ["S'"] = new(nonTerminals["S'"], new[]
            {
                new RightHandSide(nonTerminals["regex"], terminals["$"])
            }),
            ["regex"] = new(nonTerminals["regex"], new[]
            {
                new RightHandSide(nonTerminals["concat"], nonTerminals["A1"])
            }),
            ["A1"] = new(nonTerminals["A1"], new[]
            {
                new RightHandSide(terminals["|"], nonTerminals["regex"]),
                new RightHandSide(terminals["e"])
            }),
            ["concat"] = new(nonTerminals["concat"], new[]
            {
                new RightHandSide(nonTerminals["rep"], nonTerminals["A2"])
            }),
            ["A2"] = new(nonTerminals["A2"], new[]
            {
                new RightHandSide(nonTerminals["concat"]),
                new RightHandSide(terminals["e"])
            }),
            ["rep"] = new(nonTerminals["rep"], new[]
            {
                new RightHandSide(nonTerminals["atom"], nonTerminals["A3"])
            }),
            ["A3"] = new(nonTerminals["A3"], new[]
            {
                new RightHandSide(terminals["*"]),
                new RightHandSide(terminals["e"])
            }),
            ["atom"] = new(nonTerminals["atom"], new[]
            {
                new RightHandSide(terminals["("], nonTerminals["regex"], terminals[")"]),
                new RightHandSide(terminals["letter"])
            })
        };
        return new Grammar(nonTerminals, terminals, productions, nonTerminals["S'"]);
    }

    public Grammar GetExerciseGrammar()
    {
        var nonTerminals = new Dictionary<string, NonTerminal>
        {
            ["S'"] = new("S'"),
            ["S"] = new("S"),
            ["A"] = new("A")
        };
        var terminals = new Dictionary<string, Terminal>
        {
            ["$"] = new EndOfInput(),
            ["s"] = new("s"),
            ["a"] = new("a"),
            ["e"] = new Epsilon(),
        };
        var productions = new Dictionary<string, Production>
        {
            ["S'"] = new(nonTerminals["S'"], new[]
            {
                new RightHandSide(nonTerminals["S"], terminals["$"])
            }),
            ["S"] = new(nonTerminals["S"], new[]
            {
                new RightHandSide(nonTerminals["A"], terminals["s"])
            }),
            ["A"] = new(nonTerminals["A"], new[]
            {
                new RightHandSide(terminals["a"], nonTerminals["A"]),
                new RightHandSide(terminals["e"])
            })
        };
        return new Grammar(nonTerminals, terminals, productions, nonTerminals["S'"]);
    }
}
