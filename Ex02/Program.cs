using CommandLineProject.Extensions;
using Ex02.Repositories;
using Ex02.Services;
using Microsoft.Extensions.DependencyInjection;

await new ServiceCollection()
    .AddSingleton<IBerrySethiService, BerrySethiService>()
    .AddSingleton<IRegexParserService, RegexParserService>()
    .AddSingleton<IRegexTreeRepository, RegexTreeRepository>()
    .RegisterCommands(typeof(Program))
    .BuildServiceProvider()
    .RunCommandLineApp();
