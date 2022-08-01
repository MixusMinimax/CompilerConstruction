using System.Drawing;
using Ex01.Services;
using Pastel;

Console.WriteLine("Hello, World!");

IAutomata exerciseAutomata = new ExerciseAutomata();

while (true)
{
    Console.Write("$ ".Pastel(Color.Aquamarine));
    Console.OpenStandardOutput().Flush();
    var line = Console.ReadLine();
    var tokens = line?.Split(' ').Where(x => x != "").ToArray();
    var command = tokens?.ElementAtOrDefault(0);
    if (tokens is null || command is null) continue;

    switch (command.ToLower())
    {
        case "help":
            switch (tokens.ElementAtOrDefault(1))
            {
                case "help":
                    Console.WriteLine("Usage: help [command]".Pastel(Color.Cornsilk));
                    break;

                case "test":
                    Console.WriteLine("Usage: test [word]\n".Pastel(Color.Cornsilk) +
                                      "Tests if the word is a valid word for the exercise automata.".Pastel(
                                          Color.Silver));
                    break;

                case "exit":
                    Console.WriteLine("Usage: exit".Pastel(Color.Cornsilk));
                    break;

                case { } arg:
                    Console.WriteLine($"Invalid command: {arg}".Pastel(Color.Maroon));
                    break;

                case null:
                    Console.WriteLine("Available commands: help, test, exit".Pastel(Color.Cornsilk));
                    break;
            }

            break;

        case "test":
            var word = tokens.ElementAtOrDefault(1);
            if (word is null)
            {
                Console.WriteLine("Please provide a word to test");
                break;
            }

            var result = exerciseAutomata.Accepts(word, out var finalNode);
            Console.WriteLine(
                $"The word \"{word}\" was {(result ? "accepted" : "not accepted")} (Final node: {finalNode}).");
            break;

        case "exit":
            Console.WriteLine("Shutting down.".Pastel(Color.Maroon));
            goto exit;
    }
}

exit: ;