using Common.Models;

namespace Ex02.Repositories;

public interface IRegexTreeRepository
{
    public Task SaveAsync(string name, RegexTree regexTree);
    
    public Task<RegexTree?> GetAsync(string name);

    public Task<IEnumerable<(string Name, RegexTree RegexTree)>> GetAllAsync();

    public Task<bool> DeleteAsync(string name);
}

public class RegexTreeRepository : IRegexTreeRepository
{
    private readonly Dictionary<string, RegexTree> _regexTrees = new();

    public Task SaveAsync(string name, RegexTree regexTree)
    {
        _regexTrees[name] = regexTree;
        return Task.CompletedTask;
    }

    public Task<RegexTree?> GetAsync(string name)
    {
        return Task.FromResult(_regexTrees.TryGetValue(name, out var regexTree) ? regexTree : null);
    }

    public Task<IEnumerable<(string Name, RegexTree RegexTree)>> GetAllAsync()
    {
        return Task.FromResult(_regexTrees.Select(kvp => (kvp.Key, kvp.Value)));
    }

    public Task<bool> DeleteAsync(string name)
    {
        return Task.FromResult(_regexTrees.Remove(name));
    }
}
