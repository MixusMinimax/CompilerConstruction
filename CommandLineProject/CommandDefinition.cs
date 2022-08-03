using System.Windows.Input;

namespace CommandLineProject;

public record CommandDefinition(string Name, Type CommandType, Type OptionsType, Type[]? VerbTypes = default, string? HelpText = default)
{
    public Func<NonGenericCommand?>? GetImplementation { get; init; }
}