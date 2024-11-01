using System.CommandLine;

namespace DeliveryFilter;

public class RootCommandBuilder {
    public static RootCommand Build(Endpoints endpoints) {
        var rootCommand = new RootCommand("Delivery filter CLI tool");

        rootCommand.AddCommand(BuildCommandGet(endpoints));

        return rootCommand;
    }

    // ----- get ----
    protected static Command BuildCommandGet(Endpoints endpoints) {
        var getCommand = new Command("get", "Retrieve delivery data")
        {
            new Option<string>("--district", "District name"),
            new Option<DateTime>("--from-datetime", () => DateTime.MinValue, "Start date and time"),
            new Option<DateTime>("--to-datetime", () => DateTime.MaxValue, "End date and time"),
            new Option<string>("--logfile", "Log file path"),
            new Option<string>("--outfile", "Output file"),
            new Argument<string>("file", "File path"),
        };

        getCommand.SetHandler(
            endpoints.Get,
            CheckNonNull( getCommand.Options[0] as Option<string>     ), 
            CheckNonNull( getCommand.Options[1] as Option<DateTime>   ), 
            CheckNonNull( getCommand.Options[2] as Option<DateTime>   ), 
            CheckNonNull( getCommand.Options[3] as Option<string>     ), 
            CheckNonNull( getCommand.Options[4] as Option<string>     ), 
            CheckNonNull( getCommand.Arguments[0] as Argument<string> )
        );

        return getCommand;
    }

    // other
    protected static T CheckNonNull<T>(T? t) {
        if (t is null) {
            throw new Exception("Internal: value is null");
        } else {
            return t;
        }
    }
}