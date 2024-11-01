using log4net.Config;
using log4net;
using System.Reflection;
using System.CommandLine;
using Csv;


namespace DeliveryFilter;

public struct Delivery {
    public long Id;
    public string District;
    public DateTime DeliveryTime;
    public double Weight;
} 

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
            Get(parameters);
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

    public static void Get(GetParameters p) {
        foreach (var entry in ReadTableToFile(p.File)) {
            Console.WriteLine($"Entry: {entry.Id} {entry.District} {entry.DeliveryTime} {entry.Weight}");
        }
    }

    public static IEnumerable<Delivery> ReadTableToFile(string filepath) {
        return ReadTableToFile(filepath, _ => true);
    }
    public static IEnumerable<Delivery> ReadTableToFile(string filepath, Func<Delivery, bool> filter) {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);

        foreach (var line in CsvReader.Read(reader)) {
            Delivery d = ReadRowToDelivery(line);
            if (filter(d)) {
                yield return d;
            }
        }
    }

    public static Delivery ReadRowToDelivery(ICsvLine line) {
        return new Delivery
        {
            Id = long.Parse(line["Id"]),
            District = line["District"],
            DeliveryTime = DateTime.Parse(line["Datetime"]),
            Weight = double.Parse(line["Weight"])
        };
    }
}

public struct GetParameters
{
    public string District;
    public DateTime Datetime;
    public string File;
    public bool IsJson;
}