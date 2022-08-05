using System.Collections.Immutable;
using System.Text;

namespace Ex02.Models;

public record SymbolMetadata(
    Symbol Symbol,
    bool? IsEmpty = default,
    IImmutableSet<Terminal>? FEps = default,
    IImmutableSet<Terminal>? First = default,
    IImmutableSet<Terminal>? Follow = default)
{
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"{nameof(Symbol)} = {Symbol}, ");
        builder.Append($"{nameof(IsEmpty)} = {IsEmpty}, ");
        builder.Append($"{nameof(FEps)} = {{{string.Join(", ", FEps ?? Enumerable.Empty<Terminal>())}}}, ");
        builder.Append($"{nameof(First)} = {{{string.Join(", ", First ?? Enumerable.Empty<Terminal>())}}}, ");
        builder.Append($"{nameof(Follow)} = {{{string.Join(", ", Follow ?? Enumerable.Empty<Terminal>())}}}");
        return true;
    }
}