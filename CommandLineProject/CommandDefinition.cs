using System.Windows.Input;

namespace CommandLineProject;

public record CommandDefinition(string Name, Type CommandType, Type OptionsType)
{
    public Func<INonGenericCommand?>? GetImplementation { get; init; } = default;
}