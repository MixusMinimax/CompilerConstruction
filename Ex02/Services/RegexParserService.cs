using Common.Models;
using Ex02.Models;
using Epsilon = Ex02.Models.Epsilon;

namespace Ex02.Services;

public interface IRegexParserService
{
    public Task<RegexTree> ParseRegex(string regexString, StreamWriter writer);
}

public class RegexParserService : IRegexParserService
{
    private readonly IBerrySethiService _berrySethiService;

    private readonly Grammar _grammar;

    public RegexParserService(IBerrySethiService berrySethiService)
    {
        _berrySethiService = berrySethiService;
        var nonTerminals = new Dictionary<string, NonTerminal>
        {
            ["S'"] = new NonTerminal("S'"),
            ["regex"] = new NonTerminal("regex"),
            ["A1"] = new NonTerminal("A1"),
            ["concat"] = new NonTerminal("concat"),
            ["A2"] = new NonTerminal("A2"),
            ["rep"] = new NonTerminal("rep"),
            ["A3"] = new NonTerminal("A3"),
            ["atom"] = new NonTerminal("atom")
        };
        var terminals = new Dictionary<string, Terminal>
        {
            ["$"] = new Terminal("$"),
            ["letter"] = new Terminal("letter"),
            ["|"] = new Terminal("|", true),
            ["*"] = new Terminal("*", true),
            ["("] = new Terminal("(", true),
            [")"] = new Terminal(")", true),
            ["e"] = new Epsilon()
        };
        var productions = new[]
        {
            new Production(nonTerminals["S'"], new[]
            {
                new Symbol[] { nonTerminals["regex"], terminals["$"] }
            }),
            new Production(nonTerminals["regex"], new[]
            {
                new Symbol[] { nonTerminals["concat"], nonTerminals["A1"] }
            }),
            new Production(nonTerminals["A1"], new[]
            {
                new Symbol[] { terminals["|"], nonTerminals["regex"] },
                new Symbol[] { terminals["e"] }
            }),
            new Production(nonTerminals["concat"], new[]
            {
                new Symbol[] { nonTerminals["rep"], nonTerminals["A2"] }
            }),
            new Production(nonTerminals["A2"], new[]
            {
                new Symbol[] { nonTerminals["concat"] },
                new Symbol[] { terminals["e"] }
            }),
            new Production(nonTerminals["rep"], new[]
            {
                new Symbol[] { nonTerminals["atom"], nonTerminals["A3"] }
            }),
            new Production(nonTerminals["A3"], new[]
            {
                new Symbol[] { terminals["*"] },
                new Symbol[] { terminals["e"] }
            }),
            new Production(nonTerminals["atom"], new[]
            {
                new Symbol[] { terminals["("], nonTerminals["regex"], terminals[")"] },
                new Symbol[] { terminals["letter"] }
            })
        };
        _grammar = new Grammar(nonTerminals.Values, terminals.Values, productions);
    }

    public async Task<RegexTree> ParseRegex(string regexString, StreamWriter writer)
    {
        await writer.WriteLineAsync(_grammar.ToString());
        return _berrySethiService.ConstructExample();
    }
}
