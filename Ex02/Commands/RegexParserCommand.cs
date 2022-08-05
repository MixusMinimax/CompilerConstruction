using System.Drawing;
using CommandLine;
using CommandLineProject;
using Ex02.Repositories;
using Ex02.Services;
using Pastel;

namespace Ex02.Commands;

public abstract class RegexParserOptions
{
}

[Verb("save", HelpText = "Parse and save a regular expression")]
public class RegexParserSaveOptions : RegexParserOptions
{
    [Value(0, Required = true, HelpText = "The name of the regular expression")]
    public string Name { get; set; } = default!;

    [Value(1, Required = true, HelpText = "The regular expression to parse")]
    public string RegexString { get; set; } = default!;
}

[Verb("get", HelpText = "Get a regular expression")]
public class RegexParserGetOptions : RegexParserOptions
{
    [Value(0, Required = true, HelpText = "The name of the regular expression")]
    public string Name { get; set; } = default!;
}

[Verb("list", HelpText = "List all regular expressions")]
public class RegexParserListOptions : RegexParserOptions
{
}

[Verb("delete", HelpText = "Delete a regular expression")]
public class RegexParserDeleteOptions : RegexParserOptions
{
    [Value(0, Required = true, HelpText = "The name of the regular expression")]
    public string Name { get; set; } = default!;
}

public class RegexParserCommand : ICommand<
    RegexParserOptions, RegexParserSaveOptions, RegexParserGetOptions, RegexParserListOptions, RegexParserDeleteOptions>
{
    private readonly IRegexTreeRepository _regexRepository;
    private readonly IRegexParserService _regexParser;

    public RegexParserCommand(IRegexTreeRepository regexRepository, IRegexParserService regexParser)
    {
        _regexRepository = regexRepository;
        _regexParser = regexParser;
    }

    public override string Name => "regex";
    public override string HelpText => "Parse and save, retrieve, or delete regexes";

    public override async Task<int> ExecuteAsync(RegexParserSaveOptions options, StreamWriter outputWriter)
    {
        var regexTree = await _regexParser.ParseRegex(options.RegexString, outputWriter);
        await _regexRepository.SaveAsync(options.Name, regexTree);
        await outputWriter.WriteLineAsync($"Regex [{options.Name}] saved: " +
                                          $"/{regexTree.RegexString}/".Pastel(Color.DarkCyan));
        return 0;
    }

    public override async Task<int> ExecuteAsync(RegexParserGetOptions options, StreamWriter outputWriter)
    {
        var regexTree = await _regexRepository.GetAsync(options.Name);
        if (regexTree is null)
        {
            return 404;
        }

        await outputWriter.WriteLineAsync($"Regex [{options.Name}] is: " +
                                          $"/{regexTree.RegexString}/".Pastel(Color.DarkCyan));
        return 0;
    }

    public override async Task<int> ExecuteAsync(RegexParserListOptions options, StreamWriter outputWriter)
    {
        var regexes = await _regexRepository.GetAllAsync();
        foreach (var (name, regexTree) in regexes)
        {
            await outputWriter.WriteLineAsync($"{$"[{name}]:",16} " +
                                              $"/{regexTree.RegexString}/".Pastel(Color.DarkCyan));
        }

        return 0;
    }

    public override async Task<int> ExecuteAsync(RegexParserDeleteOptions options, StreamWriter outputWriter)
    {
        var deleted = await _regexRepository.DeleteAsync(options.Name);
        await outputWriter.WriteLineAsync(
            $"Regex [{options.Name}] was {(deleted ? "deleted".Pastel(Color.ForestGreen) : "not found".Pastel(Color.DarkRed))}.");
        return deleted ? 0 : 404;
    }
}
