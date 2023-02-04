using System.CommandLine;
using Datagrove.Testing.Pepinillo;

class Program
{
    public static int Main(string[] args)
    {
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "log verbosely");


        var rootCommand = new RootCommand("Pepin compiler for Cucumber language");
        //rootCommand.AddOption(fileOption);

        var buildCommand = new Command("build", "Compile cucumber into c#")
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
        await Pepin.build(Directory.GetCurrentDirectory());
    }
}
