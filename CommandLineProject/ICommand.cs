using System.Diagnostics.CodeAnalysis;

namespace CommandLineProject;

public interface INonGenericCommand
{
    public string Name { get; }
    public string? HelpText { get; }
    public Task<int> ExecuteObjectAsync(object? options, StreamWriter writer);
}

/**
 * I would like this to be an interface, but interfaces can't override methods. They just shadow them.
 */
[SuppressMessage("ReSharper", "TypeParameterCanBeVariant")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ICommand<TOptions> : INonGenericCommand where TOptions : class, new()
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions options, StreamWriter outputWriter);

    public abstract string Name { get; }
    public abstract string? HelpText { get; }

    public async Task<int> ExecuteObjectAsync(object? options, StreamWriter writer)
    {
        return await ExecuteAsync(options as TOptions ?? new TOptions(), writer);
    }
}
