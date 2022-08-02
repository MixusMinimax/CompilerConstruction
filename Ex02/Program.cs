﻿using CommandLineProject.Extensions;
using Ex02.Services;
using Microsoft.Extensions.DependencyInjection;

await new ServiceCollection()
    .AddSingleton<IBerrySethiService, BerrySethiService>()
    .RegisterCommands(typeof(Program))
    .BuildServiceProvider()
    .RunCommandLineApp();
