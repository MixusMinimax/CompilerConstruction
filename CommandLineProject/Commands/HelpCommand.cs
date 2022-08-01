namespace CommandLineProject.Commands;

public class HelpOptions
{
}

public class HelpCommand : ICommand<HelpOptions>
{
    public override string Name => "help";

    public override async Task<int> ExecuteAsync(HelpOptions options, StreamWriter writer)
    {
        await writer.WriteLineAsync("Help command");
        return 0;
    }
}