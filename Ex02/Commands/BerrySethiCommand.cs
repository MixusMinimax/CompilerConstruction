using System.Drawing;
using CommandLine;
using CommandLineProject;
using Ex02.Services;
using Pastel;

namespace Ex02.Commands;

public class BerrySethiOptions
{
    [Value(0, Required = false, HelpText = "Path to .gv file", MetaValue = "PATH")]
    public string? Path { get; set; }
}

public class BerrySethiCommand : ICommand<BerrySethiOptions>
{
    private readonly IBerriSethiService _berrySethiService;

    public BerrySethiCommand(IBerriSethiService berrySethiService)
    {
        _berrySethiService = berrySethiService;
    }


    public override async Task<int> ExecuteAsync(BerrySethiOptions options, StreamWriter writer)
    {
        var regexTree = _berrySethiService.ConstructExample();
        await writer.WriteLineAsync("RegexTree: " + $"/{regexTree.RegexString}/".Pastel(Color.DarkCyan));
        if (options.Path is null) return 0;
        await writer.WriteLineAsync("Writing to file: " + Path.GetFullPath(options.Path.Pastel(Color.DarkCyan)));
        await using var file = new FileStream(options.Path, FileMode.Create);
        await _berrySethiService.ConvertToNfa(regexTree, new StreamWriter(file));

        return 0;
    }

    public override string Name => "bsa";
    public override string HelpText => "Construct a Berry-Sethi automaton";
}
