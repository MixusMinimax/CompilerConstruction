using CommandLine;
using CommandLineProject;
using Ex03.Services;

namespace Ex03.Commands;

public class BerrySethiOptions
{
    [Option('o', "output", Required = false, HelpText = "Path to .gv file.")]
    public string? OutputPath { get; set; }

    [Value(0, Required = false, HelpText = "Word to pass to the generated dfa.")]
    public string? Word { get; set; }
}

public class BerrySethiCommand : ICommand<BerrySethiOptions>
{
    private readonly IBerrySethiService _berrySethiService;

    public BerrySethiCommand(IBerrySethiService berrySethiService)
    {
        _berrySethiService = berrySethiService;
    }

    public override string Name => "bsa";
    public override string HelpText => "BerrySethi Algorithm, converts nfa to dfa";

    public override Task<int> ExecuteAsync(BerrySethiOptions options, StreamWriter outputWriter)
    {
        throw new NotImplementedException();
    }
}
