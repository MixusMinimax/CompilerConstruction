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

    private readonly Grammar _grammar;
    private PushDownTable? _table;

    public RegexParserService(
        IBerrySethiService berrySethiService,
        IGrammarRepository grammarRepository,
        IGrammarService grammarService)
    {
        _berrySethiService = berrySethiService;
        _grammarService = grammarService;

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
                    .Select((rightHand, index) => (State: new State(nonTerminal, rightHand, 0), Index: index))
                    .ToList();
                foreach (var (expansion, choiceIndex) in expansions)
                {
                    var progressedState = state with { BulletPosition = state.BulletPosition + 1 };
                    rules.Add(new PushDownRule(PushDownRuleType.Expand, new[] { state }, Epsilon.Instance, new[]
                    {
                        state, expansion
                    }, choiceIndex));
                    rules.Add(new PushDownRule(PushDownRuleType.Reduce, new[]
                    {
                        state, expansion with { BulletPosition = expansion.RightHandSide.Length }
                    }, Epsilon.Instance, new[]
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

    public LookaheadTable GetLookaheadTable(Grammar grammar)
    {
        var metadata = _grammarService.GetMetadata(grammar);
        var rules = new Dictionary<(NonTerminal From, Terminal Lookahead), int>();

        foreach (var (from, rightHand, index) in
                 from production in grammar.Productions.Values
                 from rhs in production.RightHands.Select((rhs, i) => (RightHand: rhs, Index: i))
                 select (production.From, rhs.RightHand, rhs.Index))
        {
            var lookahead = metadata.GetFirst(rightHand.Symbols).Concatenate(metadata.GetFollow(from)!);
            foreach (var terminal in lookahead)
            {
                rules[(from, terminal)] = index;
            }
        }

        return new LookaheadTable(rules.ToImmutableDictionary(), grammar);
    }

    private async Task<PushDownTable> GetPushDownTable()
    {
        return _table ??= await CreatePushDownTable();
    }

    public async Task<RegexTree> ParseRegex(string regexString, StreamWriter writer)
    {
        await writer.WriteLineAsync(_grammar.ToString());
        var pushDownTable = await GetPushDownTable();
        await writer.WriteLineAsync(pushDownTable.ToString());
        // var metadata = _grammarService.GetMetadata(_grammar);
        // foreach (var entry in metadata)
        // {
        //     await writer.WriteLineAsync($"{entry.Key}: {entry.Value}");
        // }
        var lookaheadTable = GetLookaheadTable(_grammar);
        await writer.WriteLineAsync(lookaheadTable.ToString());

        return _berrySethiService.ConstructExample();
    }
}