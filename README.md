# Compiler Construction I (IN2227)

Usage: Go into Ex01 or Ex02 etc, and do `dotnet run`.

Then, a shell will open. type `help` for help, or `help <command>` for help on a specific command.

## Ex02

This contains Everything regarding RegexTree, including nfa, dfa, and parsing regular expression strings (TODO).

Currently, the code is able to generate a pushdown table for a given grammar (in the end it will use the grammar for regex expressions).

This is currently hardcoded into `$ regex save name regexp`, because this is where the table will later be used to parse the token stream. Lexing a regex is simple, as it is character based.