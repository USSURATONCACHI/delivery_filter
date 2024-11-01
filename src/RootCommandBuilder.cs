using System.CommandLine;

namespace DeliveryFilter;

public class RootCommandBuilder {
    public static RootCommand Build(Endpoints endpoints) {
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
            new Argument<string>("file", "File path")
        };

        genMockupCommand.SetHandler(
            endpoints.GetMockup, 
            CheckNonNull( genMockupCommand.Arguments[0] as Argument<string> )
        );

        checkCorrectnessCommand.SetHandler(
            endpoints.CheckCorrectness, 
            CheckNonNull( checkCorrectnessCommand.Arguments[0] as Argument<string> )
        );

        fixTableCommand.SetHandler(
            endpoints.FixTable, 
            CheckNonNull( fixTableCommand.Arguments[0] as Argument<string> )
        );


        getCommand.SetHandler(
            endpoints.Get,
            CheckNonNull( getCommand.Options[0] as Option<string>     ), 
            CheckNonNull( getCommand.Options[1] as Option<DateTime>   ), 
            CheckNonNull( getCommand.Arguments[0] as Argument<string> )
        );

        rootCommand.AddCommand(genMockupCommand);
        rootCommand.AddCommand(checkCorrectnessCommand);
        rootCommand.AddCommand(fixTableCommand);
        rootCommand.AddCommand(getCommand);

        return rootCommand;
    }

    protected static T CheckNonNull<T>(T? t) {
        if (t is null) {
            throw new Exception("Internal: value is null");
        } else {
            return t;
        }
    }
}