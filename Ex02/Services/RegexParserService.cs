using Common.Models;
using Ex02.Models;
using Ex02.Repositories;
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
    private PushDownTable? _table;

    public RegexParserService(IBerrySethiService berrySethiService, IGrammarRepository grammarRepository)
    {
        _berrySethiService = berrySethiService;

        _grammar = grammarRepository.GetExerciseGrammar();
    }

    private Task<PushDownTable> CreatePushDownTable()
    {
        var stateFrontier = new Stack<State>(new[]
        {
            new State(_grammar.StartSymbol,
                _grammar.Productions[_grammar.StartSymbol.Name].RightHands[0], 0)
        });
        var rules = new List<PushDownRule>();
        while (stateFrontier.Count > 0)
        {
            var state = stateFrontier.Pop();
            if (state.IsBeforeEndOfInput) continue;
            var ruleType = state.IsBeforeNonTerminal
                ? PushDownRuleType.Expand
                : PushDownRuleType.Shift;

            // If expand, immediately also add the corresponding reduce rule.
            // After shift or reduce, do not put states onto the stack where the bullet is at the end.

            if (ruleType is PushDownRuleType.Expand)
            {
                if (rules.Any(rule => rule.Head.SequenceEqual(new[] { state }))) continue;
                var nonTerminal = (NonTerminal)state.RightHandSide.Symbols[state.BulletPosition];
                var production = _grammar.Productions[nonTerminal.Name];
                var expansions = production.RightHands
                    .Select(rightHand => new State(nonTerminal, rightHand, 0))
                    .ToList();
                foreach (var expansion in expansions)
                {
                    var progressedState = state with { BulletPosition = state.BulletPosition + 1 };
                    rules.Add(new PushDownRule(PushDownRuleType.Expand, new[] { state }, new Epsilon(), new[]
                    {
                        state, expansion
                    }));
                    rules.Add(new PushDownRule(PushDownRuleType.Reduce, new[]
                    {
                        state, expansion with { BulletPosition = expansion.RightHandSide.Length }
                    }, new Epsilon(), new[]
                    {
                        progressedState
                    }));
                    if (!expansion.IsAtEnd) stateFrontier.Push(expansion);
                    if (!progressedState.IsAtEnd) stateFrontier.Push(progressedState);
                }
            }
            else
            {
                if (rules.Any(rule => rule.Head.SequenceEqual(new[] { state }))) continue;
                var terminal = (Terminal)state.RightHandSide.Symbols[state.BulletPosition];
                var progressedState = state with { BulletPosition = state.BulletPosition + 1 };
                rules.Add(
                    new PushDownRule(PushDownRuleType.Shift, new[] { state }, terminal, new[] { progressedState }));
                if (!progressedState.IsAtEnd) stateFrontier.Push(progressedState);
            }
        }

        return Task.FromResult(new PushDownTable(rules.ToArray()));
    }

    public async Task<PushDownTable> GetPushDownTable()
    {
        return _table ??= await CreatePushDownTable();
    }

    public async Task<RegexTree> ParseRegex(string regexString, StreamWriter writer)
    {
        await writer.WriteLineAsync(_grammar.ToString());
        await writer.WriteLineAsync((await GetPushDownTable()).ToString());
        return _berrySethiService.ConstructExample();
    }
}