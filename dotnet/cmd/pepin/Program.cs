using System;
using System.CommandLine;

namespace pepin;

class Program
{
    static int Main(string[] args)
    {
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "log verbosely");


        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        //rootCommand.AddOption(fileOption);

        var buildCommand = new Command("read", "Read and display the file.")
            {
                verboseOption,
            };
        rootCommand.AddCommand(buildCommand);

        buildCommand.SetHandler(async (verbose) =>
            {
                await Build(verbose);
            },
            verboseOption);

        return rootCommand.InvokeAsync(args).Result;
    }

    internal static async Task Build(bool verbose)
    {
        Console.WriteLine("Hello World!");
    }
}