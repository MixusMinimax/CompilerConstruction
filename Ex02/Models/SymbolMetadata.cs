using System.Collections.Immutable;
using System.Text;

namespace Ex02.Models;

public record SymbolMetadata(
    Symbol Symbol,
    bool? IsEmpty = default,
    IImmutableSet<Terminal>? FEps = default,
    IImmutableSet<Terminal>? FFirst = default,
    IImmutableSet<Terminal>? FFollow = default)
{
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"{nameof(Symbol)} = {Symbol}, ");
        builder.Append($"{nameof(IsEmpty)} = {IsEmpty}, ");
        builder.Append($"{nameof(FEps)} = {{{string.Join(", ", FEps ?? Enumerable.Empty<Terminal>())}}}, ");
        builder.Append($"{nameof(FFirst)} = {{{string.Join(", ", FFirst ?? Enumerable.Empty<Terminal>())}}}, ");
        builder.Append($"{nameof(FFollow)} = {{{string.Join(", ", FFollow ?? Enumerable.Empty<Terminal>())}}}");
        return true;
    }
}