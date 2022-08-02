using CommandLineProject.Extensions;
using Ex03.Services;
using Microsoft.Extensions.DependencyInjection;

await new ServiceCollection()
    .AddSingleton<IBerrySethiService, BerrySethiService>()
    .RegisterCommands(typeof(Program))
    .BuildServiceProvider()
    .RunCommandLineApp();
