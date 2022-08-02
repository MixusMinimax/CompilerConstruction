using System.Collections;
using System.Drawing;
using CommandLine;
using CommandLineProject;
using Ex02.Services;
using Pastel;

namespace Ex02.Commands;

public class BerrySethiDfaOptions
{
    [Option('o', "output", Required = false, HelpText = "Output path for graphviz file.")]
    public string? Path { get; set; }

    [Value(0, Required = true, HelpText = "Word to be tested.")]
    public string? Word { get; set; }
}

public class BerrySethiDfaCommand : ICommand<BerrySethiDfaOptions>
{
    private readonly IBerrySethiService _berrySethiService;

    private IDictionary<BitArray, IDictionary<char, BitArray>>? Transitions { get; set; }

    public BerrySethiDfaCommand(IBerrySethiService berrySethiService)
    {
        _berrySethiService = berrySethiService;
    }

    public override async Task<int> ExecuteAsync(BerrySethiDfaOptions options, StreamWriter writer)
    {
        var regexTree = _berrySethiService.ConstructExample();
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
