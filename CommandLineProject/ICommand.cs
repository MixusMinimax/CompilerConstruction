using System.Diagnostics.CodeAnalysis;

namespace CommandLineProject;

public abstract class NonGenericCommand
{
    public abstract string Name { get; }
    public abstract string? HelpText { get; }
    public abstract Task<int> ExecuteObjectAsync(object options, StreamWriter writer);
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ICommand<TOptions> : NonGenericCommand
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions options, StreamWriter outputWriter);

    public override async Task<int> ExecuteObjectAsync(object options, StreamWriter writer)
    {
        return await ExecuteAsync((TOptions)options, writer);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ICommand<TOptionsBase, TOptions1> : NonGenericCommand
    where TOptions1 : TOptionsBase
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions1 options, StreamWriter outputWriter);

    public override async Task<int> ExecuteObjectAsync(object options, StreamWriter writer)
    {
        return await ExecuteAsync((TOptions1)options, writer);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ICommand<TOptionsBase, TOptions1, TOptions2>
    : ICommand<TOptionsBase, TOptions1>
    where TOptions1 : TOptionsBase
    where TOptions2 : TOptionsBase
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions2 options, StreamWriter outputWriter);

    public override async Task<int> ExecuteObjectAsync(object options, StreamWriter writer)
    {
        return options switch
        {
            TOptions1 opt => await ExecuteAsync(opt, writer),
            TOptions2 opt => await ExecuteAsync(opt, writer),
            _ => throw new InvalidOperationException("Invalid options type")
        };
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class
    ICommand<TOptionsBase, TOptions1, TOptions2, TOptions3>
    : ICommand<TOptionsBase, TOptions1, TOptions2>
    where TOptions1 : TOptionsBase
    where TOptions2 : TOptionsBase
    where TOptions3 : TOptionsBase
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions3 options, StreamWriter outputWriter);

    public override async Task<int> ExecuteObjectAsync(object options, StreamWriter writer)
    {
        return options switch
        {
            TOptions1 opt => await ExecuteAsync(opt, writer),
            TOptions2 opt => await ExecuteAsync(opt, writer),
            TOptions3 opt => await ExecuteAsync(opt, writer),
            _ => throw new InvalidOperationException("Invalid options type")
        };
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ICommand<TOptionsBase, TOptions1, TOptions2, TOptions3, TOptions4>
    : ICommand<TOptionsBase, TOptions1, TOptions2, TOptions3>
    where TOptions1 : TOptionsBase
    where TOptions2 : TOptionsBase
    where TOptions3 : TOptionsBase
    where TOptions4 : TOptionsBase
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public abstract Task<int> ExecuteAsync(TOptions4 options, StreamWriter outputWriter);

    public override async Task<int> ExecuteObjectAsync(object options, StreamWriter writer)
    {
        return options switch
        {
            TOptions1 opt => await ExecuteAsync(opt, writer),
            TOptions2 opt => await ExecuteAsync(opt, writer),
            TOptions3 opt => await ExecuteAsync(opt, writer),
            TOptions4 opt => await ExecuteAsync(opt, writer),
            _ => throw new InvalidOperationException($"Invalid options type: {options.GetType()}")
        };
    }
}
