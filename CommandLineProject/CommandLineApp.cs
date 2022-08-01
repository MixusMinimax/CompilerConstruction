using System.Drawing;
using CommandLine;
using CommandLineProject.Exceptions;
using CommandLineProject.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Pastel;

namespace CommandLineProject;

public class CommandLineApp
{
    private Func<string, CommandDefinition?> GetCommandMapping { get; }

    public CommandLineApp(IServiceProvider serviceProvider)
    {
        GetCommandMapping = name =>
        {
            var mapping = serviceProvider.GetRequiredService<CommandMapping>().GetCommandType(name);
            if (mapping is null) return null;
            return mapping with
            {
                GetImplementation = () =>
                    serviceProvider.GetRequiredService(mapping.CommandType) as INonGenericCommand
            };
        };
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
            var mapping = GetCommandMapping(commandName);
            var command = mapping?.GetImplementation?.Invoke();
            if (mapping is null || command is null)
            {
                await writer.WriteLineAsync($"Command [{commandName}] not found".Pastel(Color.DarkRed));
                continue;
            }

            try
            {
                var options = Parser.Default.ParseArguments(mapping.OptionsType, tokens[1..]);


                var result = lastExitCode = await command.ExecuteObjectAsync(options, writer);
                if (result is not 0)
                {
                    await writer.WriteLineAsync(
                        $"Command [{commandName}] failed with code {result}".Pastel(Color.DarkRed));
                }
            }
            catch (ParserException)
            {
                lastExitCode = -1;
            }
            catch (ExitException)
            {
                break;
            }
        } while (line != null);


        await writer.FlushAsync();
    }
}