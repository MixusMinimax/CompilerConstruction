using Common.Models;

namespace Ex02.Repositories;

public interface IRegexTreeRepository
{
    public void SaveRegexTree(string name, RegexTree regexTree);

    public RegexTree? GetRegexTree(string name);

    public IEnumerable<(string Name, RegexTree RegexTree)> GetAllRegexTrees();

    public bool DeleteRegexTree(string name);
}

public class RegexTreeRepository : IRegexTreeRepository
{
    private readonly Dictionary<string, RegexTree> _regexTrees = new();

    public void SaveRegexTree(string name, RegexTree regexTree)
    {
        _regexTrees[name] = regexTree;
    }

    public RegexTree? GetRegexTree(string name)
    {
        return _regexTrees.TryGetValue(name, out var regexTree) ? regexTree : null;
    }

    public IEnumerable<(string Name, RegexTree RegexTree)> GetAllRegexTrees()
    {
        return _regexTrees.Select(kvp => (kvp.Key, kvp.Value));
    }

    public bool DeleteRegexTree(string name)
    {
        return _regexTrees.Remove(name);
    }
}
