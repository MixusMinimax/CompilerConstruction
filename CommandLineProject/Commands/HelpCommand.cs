using CommandLine;
using CommandLineProject.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CommandLineProject.Commands;

public class HelpOptions
{
    [Value(0, HelpText = "Command to provide help for", MetaName = nameof(Command))]
    public string? Command { get; set; } = default;
}

public class HelpCommand : ICommand<HelpOptions>
{
    public HelpCommand(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public override string Name => "help";
    public override string HelpText => "Show help for commands.";

    private IServiceProvider ServiceProvider { get; }

    public override async Task<int> ExecuteAsync(HelpOptions options, StreamWriter writer)
    {
        var mapping = ServiceProvider.GetRequiredService<CommandMapping>();

        if (options.Command is not null)
        {
            var result = await ServiceProvider.ExecuteCommandAsync(writer, options.Command, new[] { "--help" });
            return result == 111 ? 0 : result ?? -1;
        }

        await writer.WriteLineAsync("Available commands:");
        foreach (var command in mapping.Mapping.Values)
        {
            await writer.WriteLineAsync(
                $"  {command.Name}{(command.HelpText is not null ? $" : {command.HelpText}" : "")}");
        }

        await writer.WriteLineAsync("Help command");
        return 0;
    }
}
