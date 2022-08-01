namespace CommandLineProject;

public record CommandMapping(IDictionary<string, CommandDefinition> Mapping)
{
    public CommandDefinition? GetCommandType(string name)
        => Mapping.TryGetValue(name, out var result) ? result : null;
}