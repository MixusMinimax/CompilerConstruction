using System.Drawing;
using CommandLine;
using CommandLineProject.Exceptions;
using CommandLineProject.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pastel;

namespace CommandLineProject;

public class CommandLineApp
{
    private IServiceProvider ServiceProvider { get; }

    public CommandLineApp(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task RunAsync(Stream input, Stream output)
    {
        var reader = new StreamReader(input);
        var writer = new StreamWriter(output);
        var lastExitCode = 0;
        string? line;
        do
        {
            await writer.WriteAsync("$ ".Pastel(lastExitCode is 0 ? Color.Aquamarine : Color.Red));
            await writer.FlushAsync();
            line = await reader.ReadLineAsync();
            await writer.FlushAsync();
            var tokens = line?.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (tokens?.Length is 0 or null) continue;
            var commandName = tokens[0];

            try
            {
                var result = await ServiceProvider.ExecuteCommandAsync(writer, commandName, tokens[1..]);
                if (result is not null)
                {
                    lastExitCode = (int)result;
                }
                else continue;

                if (result is not 0)
                {
                    await writer.WriteLineAsync(
                        $"Command [{commandName}] failed with code {result}".Pastel(Color.DarkRed));
                }
            }
            catch (ExitException)
            {
                break;
            }
        } while (line is not null);


        await writer.FlushAsync();
    }
}
