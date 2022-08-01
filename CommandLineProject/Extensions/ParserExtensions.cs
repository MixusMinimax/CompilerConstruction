using CommandLine;
using CommandLineProject.Exceptions;

namespace CommandLineProject.Extensions;

public static class ParserExtensions
{
    public static object ParseArguments(this Parser parser, Type optionsType, params string[] args)
    {
        var method =
            typeof(Parser).GetMethod(nameof(Parser.ParseArguments), new[] { typeof(IEnumerable<string>) });
        var genericMethod = method!.MakeGenericMethod(optionsType);
        var parsed = genericMethod.Invoke(parser, new object[] { args })!;

        var valueGetter = parsed.GetType().GetProperty(nameof(ParserResult<object>.Value))!;
        var value = valueGetter.GetValue(parsed)!;

        if (parsed.GetType().GetGenericTypeDefinition() == typeof(NotParsed<>))
        {
            throw new ParserException();
        }

        return value;
    }
}