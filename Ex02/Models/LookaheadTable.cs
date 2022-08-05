using System.Collections.Immutable;
using System.Text;

namespace Ex02.Models;

public record LookaheadTable(ImmutableDictionary<(NonTerminal From, Terminal LookAhead), int> Rules, Grammar Grammar)
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            $"        {string.Concat(Grammar.Terminals.Values.Where(e => e is not Epsilon).Select(t => $" | {t,-8}"))}");
        sb.AppendLine(
            $"--------{string.Concat(Grammar.Terminals.Values.Where(e => e is not Epsilon).Select(_ => " | --------"))}");
        foreach (var nonTerminal in Grammar.NonTerminals.Values)
        {
            sb.Append($"{nonTerminal,8}");

            foreach (var terminal in Grammar.Terminals.Values.Where(e => e is not Epsilon))
            {
                var rule = Rules.GetValueOrDefault((nonTerminal, terminal), -1);
                sb.Append($" | {(rule is not -1 ? rule : ""),8}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}