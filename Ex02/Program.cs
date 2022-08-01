using CommandLineProject;
using CommandLineProject.Extensions;
using Microsoft.Extensions.DependencyInjection;

await new ServiceCollection()
    .RegisterCommands(typeof(Program))
    .BuildServiceProvider()
    .RunCommandLineApp();