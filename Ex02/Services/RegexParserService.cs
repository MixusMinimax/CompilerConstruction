using Common.Models;

namespace Ex02.Services;

public interface IRegexParserService
{
    public RegexTree ParseRegex(string regexString);
}

public class RegexParserService : IRegexParserService
{
    public RegexTree ParseRegex(string regexString)
    {
        throw new NotImplementedException();
    }
}
