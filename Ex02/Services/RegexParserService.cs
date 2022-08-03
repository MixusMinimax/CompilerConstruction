using Common.Models;

namespace Ex02.Services;

public interface IRegexParserService
{
    public RegexTree ParseRegex(string regexString);
}

public class RegexParserService : IRegexParserService
{
    private readonly IBerrySethiService _berrySethiService;

    public RegexParserService(IBerrySethiService berrySethiService)
    {
        _berrySethiService = berrySethiService;
    }

    public RegexTree ParseRegex(string regexString)
    {
        return _berrySethiService.ConstructExample();
    }
}
