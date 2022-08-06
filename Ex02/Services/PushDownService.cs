using System.Collections.Immutable;
using Common;
using Ex02.Models;
using Ex02.Util;

namespace Ex02.Services;

public interface IPushDownService
{
    public PushDownAutomaton CreatePushDownAutomaton(Grammar grammar);

    public bool RunPushDownAutomaton(PushDownAutomaton pushDownAutomaton, IEnumerable<Token> tokensEnumerable,
        out DerivationItem derivation);
}

public class PushDownService : IPushDownService
{
    private readonly IGrammarService _grammarService;
    private readonly ICache<Grammar, PushDownAutomaton> _cache = new Cache<Grammar, PushDownAutomaton>();

    public PushDownService(IGrammarService grammarService)
    {
        _grammarService = grammarService;
    }

    public PushDownAutomaton CreatePushDownAutomaton(Grammar grammar)
    {
        if (_cache.TryGetValue(grammar, out var result)) return result;

        var metadata = _grammarService.GetMetadata(grammar);
        var pushDownTable = CreatePushDownTable(grammar);
        var lookaheadTable = CreateLookaheadTable(grammar);
        return _cache.Set(grammar, new PushDownAutomaton(grammar, metadata, pushDownTable, lookaheadTable));
    }

    public bool RunPushDownAutomaton(PushDownAutomaton pushDownAutomaton, IEnumerable<Token> tokensEnumerable,
        out DerivationItem derivation)
    {
        derivation = new ExpansionDerivationItem(pushDownAutomaton.PushDownTable.StartState.NonTerminal,
            0, ImmutableList<DerivationItem>.Empty);

        var derivationStack = new Stack<ExpansionDerivationItem>();
        derivationStack.Push((ExpansionDerivationItem)derivation);

        var stack = new Stack<State>();
        stack.Push(pushDownAutomaton.PushDownTable.StartState);

        var tokens = tokensEnumerable.ToList();

        Token? token = null;
        for (var i = 0; i < tokens.Count;)
        {
            if (token is null)
            {
                token = tokens[0];
                i = 0;
                continue;
            }

            var candidateRules = pushDownAutomaton.PushDownTable.Rules
                .Where(rule => rule.Terminal == token.Terminal || rule.Terminal is Epsilon)
                .Where(rule => rule.Head.Length <= stack.Count)
                .Where(rule => rule.Head.Reverse().Zip(stack).All(pair => pair.First == pair.Second))
                .ToList();

            if (candidateRules.Count == 0) break;

            PushDownRule rule;
            if (candidateRules.Count == 1)
            {
                rule = candidateRules.First();
            }
            else
            {
                var nonTerminal = candidateRules.First().ChoiceNonTerminal!;
                var lookaheadIndexExists =
                    pushDownAutomaton.LookaheadTable.Rules.TryGetValue((nonTerminal, token.Terminal),
                        out var lookaheadIndex);
                if (!lookaheadIndexExists) break;
                rule = candidateRules.First(r => r.ChoiceIndex == lookaheadIndex);
            }

            foreach (var _ in rule.Head)
            {
                stack.Pop();
            }

            foreach (var state in rule.Result)
            {
                stack.Push(state);
            }

            ExpansionDerivationItem? top;
            switch (rule.RuleType)
            {
                case PushDownRuleType.Expand:
                    derivationStack.Push(new ExpansionDerivationItem(rule.Result[^1].NonTerminal,
                        rule.ChoiceIndex ?? 0, ImmutableList<DerivationItem>.Empty));
                    break;
                case PushDownRuleType.Shift:
                    top = derivationStack.Pop();
                    derivationStack.Push(top with
                    {
                        Children = top.Children.Add(new ShiftDerivationItem(token))
                    });
                    token = tokens[++i];
                    break;
                case PushDownRuleType.Reduce:
                    var newChild = derivationStack.Pop();
                    top = derivationStack.Pop();
                    derivationStack.Push(top with
                    {
                        Children = top.Children.Add(newChild)
                    });
                    break;
            }
        }

        derivation = derivationStack.Pop();

        return stack.Count == 1 && token?.Terminal == pushDownAutomaton.Grammar.Terminals["$"] &&
               stack.Peek() == pushDownAutomaton.PushDownTable.FinishState;
    }

    private PushDownTable CreatePushDownTable(Grammar grammar)
    {
        var startState = new State(grammar.StartSymbol,
            grammar.Productions[grammar.StartSymbol.Name].RightHands[0], 0);
        var stateFrontier = new Stack<State>(new[]
        {
            startState
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
                var production = grammar.Productions[nonTerminal.Name];
                var expansions = production.RightHands
                    .Select((rightHand, index) => (State: new State(nonTerminal, rightHand, 0), Index: index))
                    .ToList();
                foreach (var (expansion, choiceIndex) in expansions)
                {
                    var progressedState = state with { BulletPosition = state.BulletPosition + 1 };
                    rules.Add(new PushDownRule(PushDownRuleType.Expand, new[] { state }, Epsilon.Instance, new[]
                    {
                        state, expansion
                    }, choiceIndex, expansion.NonTerminal));
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

        return new PushDownTable(rules.ToArray(),
            startState,
            startState with { BulletPosition = startState.BulletPosition + 1 });
    }

    private LookaheadTable CreateLookaheadTable(Grammar grammar)
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
}