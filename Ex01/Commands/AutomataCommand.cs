using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CommandLine;
using CommandLineProject;
using Ex01.Services;
using Pastel;

namespace Ex01.Commands;

public class AutomataOptions
{
    [Value(0, Required = true, MetaName = nameof(Word))]
    public string Word { get; set; } = null!;
}

public class AutomataCommand : ICommand<AutomataOptions>
{
    private readonly IAutomata _exerciseAutomata;

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
    public AutomataCommand(ExerciseAutomata exerciseAutomata)
    {
        _exerciseAutomata = exerciseAutomata;
    }

    public override string Name => "test";
    public override string HelpText => "Tests if the word is a valid word for the exercise automata.";

    public override async Task<int> ExecuteAsync(AutomataOptions options, StreamWriter outputWriter)
    {
        var result = _exerciseAutomata.Accepts(options.Word, out var finalNode);
        await outputWriter.WriteLineAsync(
            $"The word \"{options.Word}\" was {(result ? "accepted".Pastel(Color.ForestGreen) : "not accepted".Pastel(Color.Coral))} (Final node: {finalNode}).");
        return 0;
    }
}
