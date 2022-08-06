using Common.Extensions;

namespace Ex02.Models;

public enum PushDownRuleType
{
    Expand,
    Shift,
    Reduce
}

public static class PushDownRuleTypeExtensions
{
    public static string ToSimpleString(this PushDownRuleType instance) => instance switch
    {
        PushDownRuleType.Expand => "e",
        PushDownRuleType.Shift => "s",
        PushDownRuleType.Reduce => "r",
        _ => throw new ArgumentOutOfRangeException(nameof(instance), instance, null)
    };
}

public record PushDownRule(PushDownRuleType RuleType, State[] Head, Terminal Terminal, State[] Result,
    int? ChoiceIndex = default, NonTerminal? ChoiceNonTerminal = default)
{
    public override string ToString() =>
        $"{RuleType.ToSimpleString()} | {string.Join<State>(' ', Head),64} | {Terminal.Center(8)} | {string.Join<State>(' ', Result)}" +
        (ChoiceIndex is not null ? $" | {ChoiceIndex}" : "") +
        (ChoiceNonTerminal is not null ? $" | {ChoiceNonTerminal}" : "");
}

public record PushDownTable(
    PushDownRule[] Rules,
    State StartState,
    State FinishState)
{
    public override string ToString() =>
        string.Join('\n', Rules.Select((rule, i) => $"{i,3} {rule}"));
}