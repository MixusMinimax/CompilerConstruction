using CommandLineProject.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                  type.BaseType?.GetGenericTypeDefinition() == typeof(ICommand<>)
            select new CommandDefinition(
                type.Name,
                type,
                type
                    .BaseType!
                    .GetGenericArguments()[0]
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
        foreach (var (_, definition) in mapping.Mapping.ToList())
        {
            var instance = (INonGenericCommand)services.GetRequiredService(definition.CommandType);
            mapping.Mapping[instance.Name] = definition with { Name = instance.Name };
        }

        var app = services.GetRequiredService<CommandLineApp>();
        await app.RunAsync(input ?? Console.OpenStandardInput(), output ?? Console.OpenStandardOutput());
    }
}