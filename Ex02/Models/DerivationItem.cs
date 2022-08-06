using System.Collections.Immutable;

namespace Ex02.Models;

public abstract record DerivationItem;

public record ExpansionDerivationItem
    (NonTerminal From, RightHandSide RightHandSide, ImmutableList<DerivationItem> Children) : DerivationItem;

public record ShiftDerivationItem(Token Token) : DerivationItem;