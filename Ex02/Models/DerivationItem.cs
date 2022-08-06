namespace Ex02.Models;

public abstract record DerivationItem;

public record ExpansionDerivationItem(NonTerminal From, RightHandSide RightHandSide) : DerivationItem;

public record ShiftDericationItem(Token Token) : DerivationItem;