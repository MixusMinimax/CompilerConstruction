using System.Drawing;
using CommandLine;
using CommandLineProject.Commands;
using CommandLineProject.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pastel;

namespace CommandLineProject.Extensions;

public static class CommandExtensions
{
    public static IServiceCollection RegisterCommands(this IServiceCollection services, params Type[] markers)
    {
        var commandTypes = (
            from marker in markers
            from type in marker.Assembly.ExportedTypes
            where !type.IsInterface && !type.IsAbstract
            where (type.BaseType?.IsGenericType ?? false) &&
                  (type.BaseType!.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                   type.BaseType!.GetGenericTypeDefinition() == typeof(ICommand<,>) ||
                   type.BaseType!.GetGenericTypeDefinition() == typeof(ICommand<,,>) ||
                   type.BaseType!.GetGenericTypeDefinition() == typeof(ICommand<,,,>) ||
                   type.BaseType!.GetGenericTypeDefinition() == typeof(ICommand<,,,,>))
            select new CommandDefinition(
                type.Name,
                type,
                type
                    .BaseType!
                    .GetGenericArguments()[0],
                VerbTypes: type.BaseType!.GetGenericArguments().Length == 1
                    ? null
                    : type
                        .BaseType!
                        .GetGenericArguments()[1..]
            )
        ).ToDictionary(e => e.Name);

        commandTypes[nameof(HelpCommand)] =
            new CommandDefinition(nameof(HelpCommand), typeof(HelpCommand), typeof(HelpOptions));
        commandTypes[nameof(ExitCommand)] =
            new CommandDefinition(nameof(ExitCommand), typeof(ExitCommand), typeof(ExitOptions));

        services.TryAdd(
            from command in commandTypes.Values
            select new ServiceDescriptor(command.CommandType, command.CommandType, ServiceLifetime.Singleton)
        );

        services.AddSingleton<CommandLineApp>();

        return services.AddSingleton(new CommandMapping(commandTypes));
    }

    public static async Task RunCommandLineApp(this IServiceProvider services, Stream? input = default,
        Stream? output = default)
    {
        var mapping = services.GetRequiredService<CommandMapping>();
        foreach (var (old, definition) in mapping.Mapping.ToList())
        {
            var instance = (NonGenericCommand)services.GetRequiredService(definition.CommandType);
            mapping.Mapping.Remove(old);
            mapping.Mapping[instance.Name] = definition with { Name = instance.Name, HelpText = instance.HelpText };
        }

        var app = services.GetRequiredService<CommandLineApp>();
        await app.RunAsync(input ?? Console.OpenStandardInput(), output ?? Console.OpenStandardOutput());
    }

    public static CommandDefinition? GetCommandDefinition(this IServiceProvider serviceProvider, string name)
    {
        var mapping = serviceProvider.GetRequiredService<CommandMapping>().GetCommandType(name);
        if (mapping is null) return null;
        return mapping with
        {
            GetImplementation = () =>
                serviceProvider.GetRequiredService(mapping.CommandType) as NonGenericCommand
        };
    }

    public static async Task<int?> ExecuteCommandAsync(this IServiceProvider services, StreamWriter writer,
        string commandName, IEnumerable<string> arguments)
    {
        var mapping = services.GetCommandDefinition(commandName);
        var command = mapping?.GetImplementation?.Invoke();
        if (mapping is null || command is null)
        {
            await writer.WriteLineAsync($"Command [{commandName}] not found".Pastel(Color.DarkRed));
            return 404;
        }

        var args = arguments as string[] ?? arguments.ToArray();
        var isHelp = args.Contains("--help");

        try
        {
            if (isHelp)
            {
                await writer.WriteLineAsync($"{commandName} : {command.HelpText}".Pastel(Color.DarkCyan));
            }

            var parser = new Parser(settings => { settings.HelpWriter = writer; });
            var options = (mapping.VerbTypes is not null
                    ? parser.ParseArguments(args, mapping.VerbTypes)
                    : parser.ParseArguments(mapping.OptionsType, args))
                .WithNotParsed(_ => throw new ParserException()).Value;
            return await command.ExecuteObjectAsync(options, writer);
        }
        catch (ParserException)
        {
            return isHelp ? 0 : 111;
        }
    }
}
