using System.CommandLine;

namespace Pepin;

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

        var initCommand = new Command("init", "Initialize pepin")
            {
                verboseOption,
            };
        rootCommand.AddCommand(initCommand);

        initCommand.SetHandler(async (verbose) =>
            {
                await Init(verbose);
            },
            verboseOption);


        return rootCommand.InvokeAsync(args).Result;
    }
    internal static async Task Init(bool verbose)
    {
        await Datagrove.Pep.Pepin.init(Directory.GetCurrentDirectory());

    }
    internal static async Task Build(bool verbose)
    {
        await Datagrove.Pep.Pepin.build(Directory.GetCurrentDirectory());
    }
}
