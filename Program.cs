using log4net.Config;
using log4net;
using System.Reflection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;


namespace DeliveryFilter;

internal class Program
{
    private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

    static int Main(string[] args)
    {
        if (Assembly.GetEntryAssembly() is Assembly asm) {
            var logRepository = LogManager.GetRepository(asm);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
        LOG.Info("Program starts");

        var rootCommand = new RootCommand("Delivery filter CLI tool");

        var genMockupCommand = new Command("gen_mockup", "Generates a mockup")
        { new Argument<string>("file", "File path") };
        
        var checkCorrectnessCommand = new Command("check_correctness", "Checks correctness") 
        { new Argument<string>("file", "File path") };
        
        var fixTableCommand = new Command("fix_table", "Fixes table") 
        { new Argument<string>("file", "File path") };

        var getCommand = new Command("get", "Retrieve delivery data")
        {
            new Option<string>("--district", "District name"),
            new Option<DateTime>("--datetime", () => DateTime.MinValue, "Date and time"),
            new Option<bool>("--json", "Output as JSON"),
            new Argument<string>("file", "File path")
        };

        genMockupCommand.SetHandler((string file) =>
        {
            Console.WriteLine($"Generating mockup for {file}");
        }, genMockupCommand.Arguments[0] as Argument<string>);

        checkCorrectnessCommand.SetHandler((string file) =>
        {
            Console.WriteLine($"Checking correctness of {file}");
        }, checkCorrectnessCommand.Arguments[0] as Argument<string>);

        fixTableCommand.SetHandler((string file) =>
        {
            Console.WriteLine($"Fixing table in {file}");
        }, fixTableCommand.Arguments[0] as Argument<string>);

        getCommand.SetHandler((string district, DateTime datetime, bool json, string file) =>
        {
            var parameters = new GetParameters
            {
                District = district,
                Datetime = datetime,
                IsJson = json,
                File = file
            };

            Console.WriteLine($"Getting data for district: {parameters.District}, datetime: {parameters.Datetime}, JSON output: {parameters.IsJson}, file: {parameters.File}");
        },
        getCommand.Options[0] as Option<string>, 
        getCommand.Options[1] as Option<DateTime>, 
        getCommand.Options[2] as Option<bool>, 
        getCommand.Arguments[0] as Argument<string>);

        rootCommand.AddCommand(genMockupCommand);
        rootCommand.AddCommand(checkCorrectnessCommand);
        rootCommand.AddCommand(fixTableCommand);
        rootCommand.AddCommand(getCommand);

        return rootCommand.Invoke(args);

    }
}

public struct GetParameters
{
    public string District;
    public DateTime Datetime;
    public string File;
    public bool IsJson;
}



        /*
        
        $ delivery_filter gen_mockup <file>
        $ delivery_filter check_correctness <file>
        $ delivery_filter fix_table <file>

        $ delivery_filter get Stillwater 2024-01-10 12:52:17 <file>
        $ delivery_filter get Stillwater 2024-01-10 12:52:17 --json <file>
        $ delivery_filter get --district Stillwater --date 2024-01-10 --time 12:52:17 <file>
        $ delivery_filter get --district Stillwater --datetime "2024-01-10 12:52:17" <file>
        
        */