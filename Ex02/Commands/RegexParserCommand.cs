using CommandLine;
using CommandLineProject;
using Ex02.Repositories;
using Ex02.Services;

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
    
    public override Task<int> ExecuteAsync(RegexParserSaveOptions options, StreamWriter outputWriter)
    {
        throw new NotImplementedException();
    }

    public override Task<int> ExecuteAsync(RegexParserGetOptions options, StreamWriter outputWriter)
    {
        throw new NotImplementedException();
    }

    public override Task<int> ExecuteAsync(RegexParserListOptions options, StreamWriter outputWriter)
    {
        throw new NotImplementedException();
    }

    public override Task<int> ExecuteAsync(RegexParserDeleteOptions options, StreamWriter outputWriter)
    {
        throw new NotImplementedException();
    }
}
