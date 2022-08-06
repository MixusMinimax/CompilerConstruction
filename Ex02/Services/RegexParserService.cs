using System.Collections.Immutable;
using Common.Models;
using Ex02.Exceptions;
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

    private RegexTree? ConvertAstToRegex(Grammar grammar, DerivationItem root)
    {
        if (root is ShiftDerivationItem shift)
        {
            if (shift.Token.Terminal == grammar.Terminals["letter"])
                return shift.Token.Value switch
                {
                    "()" => new Common.Models.Epsilon(),
                    { Length: 1 } x => new Letter(x[0]),
                    _ => throw new RegexConversionException()
                };
            throw new RegexConversionException();
        }

        if (root is not ExpansionDerivationItem expansion) throw new RegexConversionException();

        RegexTree? left;
        RegexTree? right;
        switch (expansion)
        {
            case { From.Name : "atom", ProductionIndex: 0 }:
                return ConvertAstToRegex(grammar, expansion.Children[1]);
            case { From.Name: "atom", ProductionIndex: 1 }:
                return ConvertAstToRegex(grammar, expansion.Children[0]);
            case { From.Name: "A3", ProductionIndex: 0 }:
                return new Star(new Common.Models.Epsilon());
            case { From.Name: "A3", ProductionIndex: 1 }:
                return null;
            case { From.Name: "rep" }:
                var subExpr = ConvertAstToRegex(grammar, expansion.Children[0]);
                return ConvertAstToRegex(grammar, expansion.Children[1]) is Star
                    ? new Star(subExpr!)
                    : subExpr;
            case { From.Name: "A2", ProductionIndex: 0 }:
                return ConvertAstToRegex(grammar, expansion.Children[0]);
            case { From.Name: "A2", ProductionIndex: 1 }:
                return null;
            case { From.Name: "concat" }:
                left = ConvertAstToRegex(grammar, expansion.Children[0]);
                right = ConvertAstToRegex(grammar, expansion.Children[1]);
                return right is not null ? new Concat(left!, right) : left;
            case { From.Name: "A1", ProductionIndex: 0 }:
                return ConvertAstToRegex(grammar, expansion.Children[1]);
            case { From.Name: "A1", ProductionIndex: 1 }:
                return null;
            case { From.Name: "regex" }:
                left = ConvertAstToRegex(grammar, expansion.Children[0]);
                right = ConvertAstToRegex(grammar, expansion.Children[1]);
                return right is not null ? new Or(left!, right) : left;
            case { From.Name: "S'" }:
                return ConvertAstToRegex(grammar, expansion.Children[0]);
            default: throw new RegexConversionException();
        }
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

        if (!accepted)
        {
            throw new ArgumentException("Regex string is invalid!");
        }

        return ConvertAstToRegex(_grammar, derivation)!;
    }
}