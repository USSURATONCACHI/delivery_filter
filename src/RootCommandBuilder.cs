using System.CommandLine;

namespace DeliveryFilter;

public class RootCommandBuilder {
    public static RootCommand Build(Endpoints endpoints) {
        var rootCommand = new RootCommand("Delivery filter CLI tool");

        rootCommand.AddCommand(BuildCommandGetMockup(endpoints));
        rootCommand.AddCommand(BuildCommandCheckCorrectness(endpoints));
        rootCommand.AddCommand(BuildCommandFixTable(endpoints));
        rootCommand.AddCommand(BuildCommandGet(endpoints));

        return rootCommand;
    }

    // ----- gen_mockup ----
    protected static Command BuildCommandGetMockup(Endpoints endpoints) {
        var genMockupCommand = new Command("gen_mockup", "Generates a mockup")
        { 
            new Argument<string>("file", "File path") 
        };

        genMockupCommand.SetHandler(
            endpoints.GetMockup, 
            CheckNonNull( genMockupCommand.Arguments[0] as Argument<string> )
        );

        return genMockupCommand;
    }

    // ----- fix_table ----
    protected static Command BuildCommandFixTable(Endpoints endpoints) {
        var fixTableCommand = new Command("fix_table", "Fixes table") 
        { 
            new Argument<string>("file", "File path") 
        };

        fixTableCommand.SetHandler(
            endpoints.FixTable, 
            CheckNonNull( fixTableCommand.Arguments[0] as Argument<string> )
        );

        return fixTableCommand;
    }
    
    // ----- check_correctness ----
    protected static Command BuildCommandCheckCorrectness(Endpoints endpoints) {
        var checkCorrectnessCommand = new Command("check_correctness", "Checks correctness") 
        { 
            new Argument<string>("file", "File path") 
        };

        checkCorrectnessCommand.SetHandler(
            endpoints.CheckCorrectness, 
            CheckNonNull( checkCorrectnessCommand.Arguments[0] as Argument<string> )
        );

        return checkCorrectnessCommand;
    }

    // ----- get ----
    protected static Command BuildCommandGet(Endpoints endpoints) {
        var getCommand = new Command("get", "Retrieve delivery data")
        {
            new Option<string>("--district", "District name"),
            new Option<DateTime>("--datetime", () => DateTime.MinValue, "Date and time"),
            new Option<string>("--logfile", "Log file path"),
            new Option<string>("--outfile", "Output file"),
            new Argument<string>("file", "File path"),
        };

        getCommand.SetHandler(
            endpoints.Get,
            CheckNonNull( getCommand.Options[0] as Option<string>     ), 
            CheckNonNull( getCommand.Options[1] as Option<DateTime>   ), 
            CheckNonNull( getCommand.Options[2] as Option<string>     ), 
            CheckNonNull( getCommand.Options[3] as Option<string>     ), 
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