using System.Collections;
using System.Drawing;
using CommandLine;
using CommandLineProject;
using Ex02.Repositories;
using Ex02.Services;
using Pastel;

namespace Ex02.Commands;

public class BerrySethiDfaOptions
{
    [Option('o', "output", Required = false, HelpText = "Output path for graphviz file.")]
    public string? Path { get; set; }

    [Option('r', "regex", Required = false, Default = "example", HelpText = "Name of the regex to use.")]
    public string RegexName { get; set; } = null!;

    [Value(0, Required = true, HelpText = "Word to be tested.")]
    public string? Word { get; set; }
}

public class BerrySethiDfaCommand : ICommand<BerrySethiDfaOptions>
{
    private readonly IBerrySethiService _berrySethiService;
    private readonly IRegexTreeRepository _regexTreeRepository;

    private IDictionary<BitArray, IDictionary<char, BitArray>>? Transitions { get; set; }

    public BerrySethiDfaCommand(IBerrySethiService berrySethiService, IRegexTreeRepository regexTreeRepository)
    {
        _berrySethiService = berrySethiService;
        _regexTreeRepository = regexTreeRepository;
    }

    public override async Task<int> ExecuteAsync(BerrySethiDfaOptions options, StreamWriter writer)
    {
        var regexTree = await _regexTreeRepository.GetAsync(options.RegexName);
        if (regexTree is null)
        {
            await writer.WriteLineAsync($"Regex [{options.RegexName}] not found.");
            return 404;
        }

        await writer.WriteLineAsync("RegexTree: " + $"/{regexTree.RegexString}/".Pastel(Color.DarkCyan));

        if (options.Path is not null)
        {
            await writer.WriteLineAsync("Writing to file: " + Path.GetFullPath(options.Path.Pastel(Color.DarkCyan)));
            await using var file = new FileStream(options.Path, FileMode.Create);

            Transitions = await _berrySethiService.ConvertToDfa(regexTree,
                writer, new StreamWriter(file), options.Word!, Transitions);
        }
        else
        {
            Transitions = await _berrySethiService.ConvertToDfa(regexTree,
                writer, new StringWriter(), options.Word!, Transitions);
        }

        return 0;
    }

    public override string Name => "dfa";
    public override string HelpText => "Convert RegexTree to DFA";
}