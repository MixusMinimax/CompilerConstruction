using CommandLineProject.Exceptions;

namespace CommandLineProject.Commands;

public class ExitOptions
{
}

public class ExitCommand : ICommand<ExitOptions>
{
    public override string Name => "exit";

    public override Task<int> ExecuteAsync(ExitOptions options, StreamWriter outputWriter)
    {
        outputWriter.WriteLine("Exiting...");
        throw new ExitException();
    }
}