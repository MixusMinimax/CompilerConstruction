using CommandLineProject.Extensions;
using Ex02.Services;
using Microsoft.Extensions.DependencyInjection;

await new ServiceCollection()
    .AddSingleton<IBerriSethiService, BerriSethiService>()
    .RegisterCommands(typeof(Program))
    .BuildServiceProvider()
    .RunCommandLineApp();
