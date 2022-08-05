using System.Reflection;
using CommandLine;
using TypeInfo = CommandLine.TypeInfo;

namespace CommandLineProject.Extensions;

public static class ParserExtensions
{
    private class SimpleTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.Assembly == y.Assembly &&
                   x.Namespace == y.Namespace &&
                   x.Name == y.Name;
        }

        public int GetHashCode(Type obj)
        {
            throw new NotImplementedException();
        }
    }

    public static ParserResult<object> ParseArguments(this Parser parser, Type optionsType, IEnumerable<string> args)
    {
        var method =
            typeof(Parser).GetMethod(nameof(Parser.ParseArguments), new[] { typeof(IEnumerable<string>) });
        var genericMethod = method!.MakeGenericMethod(optionsType);
        var parsed = genericMethod.Invoke(parser, new object[] { args })!;

        var parameters = typeof(ParserResultExtensions).GetMethods()
            .First(e => e.Name == nameof(ParserResultExtensions.MapResult)).GetParameters().Select(e => e.ParameterType)
            .ToList();

        var parsedConstructor = typeof(Parsed<object>).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            new[] { typeof(object), typeof(TypeInfo) })!;
        var notParsedConstructor = typeof(NotParsed<object>).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            new[] { typeof(TypeInfo), typeof(IEnumerable<Error>) })!;
        var valueProperty = parsed.GetType().BaseType!.GetProperty("Value")!;
        var typeInfoProperty = parsed.GetType().BaseType!.GetProperty("TypeInfo")!;
        var errorsProperty = parsed.GetType().BaseType!.GetProperty("Errors")!;

        if (parsed.GetType().GetGenericTypeDefinition() == typeof(Parsed<>))
        {
            return (ParserResult<object>)parsedConstructor.Invoke(new[]
                { valueProperty.GetValue(parsed), typeInfoProperty.GetValue(parsed) });
        }

        if (parsed.GetType().GetGenericTypeDefinition() == typeof(NotParsed<>))
        {
            return (ParserResult<object>)notParsedConstructor.Invoke(new[]
                { typeInfoProperty.GetValue(parsed), errorsProperty.GetValue(parsed) });
        }

        throw new InvalidOperationException("Unexpected ParserResult type");
    }
}
