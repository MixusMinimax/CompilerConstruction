using System.Collections.Immutable;
using Common.Models;
using Ex02.Models;
using Ex02.Repositories;
using Ex02.Util;
using Epsilon = Ex02.Models.Epsilon;

namespace Ex02.Services;

public interface IRegexParserService
{
    public Task<RegexTree> ParseRegex(string regexString, StreamWriter writer);
}

public class RegexParserService : IRegexParserService
{
    private readonly IBerrySethiService _berrySethiService;
    private readonly IGrammarService _grammarService;
    private readonly IPushDownService _pushDownService;

    private readonly Grammar _grammar;

    public RegexParserService(
        IBerrySethiService berrySethiService,
        IGrammarRepository grammarRepository,
        IGrammarService grammarService,
        IPushDownService pushDownService)
    {
        _berrySethiService = berrySethiService;
        _grammarService = grammarService;
        _pushDownService = pushDownService;

        _grammar = grammarRepository.GetRegexGrammar();
    }

    public async Task<RegexTree> ParseRegex(string regexString, StreamWriter writer)
    {
        await writer.WriteLineAsync(_grammar.ToString());
        var pushDownAutomaton = _pushDownService.CreatePushDownAutomaton(_grammar);
        await writer.WriteLineAsync(pushDownAutomaton.PushDownTable.ToString());
        await writer.WriteLineAsync(pushDownAutomaton.LookaheadTable.ToString());

        var regexTokens = new List<Token>();
        while (regexString.Length > 0)
        {
            if (regexString.StartsWith("()"))
            {
                regexTokens.Add(new Token(_grammar.Terminals["letter"], "()"));
                regexString = regexString[2..];
            }
            else if (regexString.StartsWith('('))
            {
                regexTokens.Add(new Token(_grammar.Terminals["("], "("));
                regexString = regexString[1..];
            }
            else if (regexString.StartsWith(')'))
            {
                regexTokens.Add(new Token(_grammar.Terminals[")"], ")"));
                regexString = regexString[1..];
            }
            else if (regexString.StartsWith('*'))
            {
                regexTokens.Add(new Token(_grammar.Terminals["*"], "*"));
                regexString = regexString[1..];
            }
            else if (regexString.StartsWith('|'))
            {
                regexTokens.Add(new Token(_grammar.Terminals["|"], "|"));
                regexString = regexString[1..];
            }
            else
            {
                regexTokens.Add(new Token(_grammar.Terminals["letter"], regexString[0].ToString()));
                regexString = regexString[1..];
            }
        }

        var accepted = _pushDownService.RunPushDownAutomaton(pushDownAutomaton,
            regexTokens.Append(new Token(_grammar.Terminals["$"], "$")),
            out var derivation);

        await writer.WriteLineAsync($"{regexString} was {(accepted ? "accepted" : "rejected")}");

        return _berrySethiService.ConstructExample();
    }
}