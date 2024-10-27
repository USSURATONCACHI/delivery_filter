using log4net.Config;
using log4net;
using System.Reflection;
using DeliveryFilter.Parse;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;

namespace DeliveryFilter;

internal class Program
{
    private static readonly string PROGRAM_NAME = "delivery_filter";
    private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

    static int Main(string[] args)
    {
        if (Assembly.GetEntryAssembly() is Assembly asm) {
            var logRepository = LogManager.GetRepository(asm);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
        LOG.Info("Program starts");

        string[] parse_args = args;
        ArgsParser.ParseResult result = ArgsParser.ParseAndCheck(parse_args);

        while (result.Error != "") {
            PrintParseResultMetadata(result, parse_args);

            if (result.FixedArgs is null)
                break;
            parse_args = result.FixedArgs;

            result = ArgsParser.ParseAndCheck(parse_args);
        }

        return 0;
    }

    private static void PrintParseResultMetadata(ArgsParser.ParseResult result, string[] args) {
        if (result.Error != "") {
            PrintError($"Error: {result.Error}");

            if (result.ErrorFocus != -1)
                PrintWithFocusOnArgument(args, result.ErrorFocus, ConsoleColor.Red);
        }

        if (result.FixExplanation != "") {
            PrintSuggestion($"Suggestion: {result.FixExplanation}");

            if (result.FixInputFocus != -1)
                PrintWithFocusOnArgument(args, result.FixInputFocus, ConsoleColor.Blue);
            
            if (result.FixedArgs is not null) {
                if (result.FixOuputFocus != -1)
                    PrintWithFocusOnArgument(result.FixedArgs, result.FixOuputFocus, ConsoleColor.Green);
                else
                    PrintWithoutFocus(result.FixedArgs);
            }
        }
    }

    private static void Help() {
        Console.WriteLine($"Usage:   $ {PROGRAM_NAME} get <district> <date> <time> <file>");
        Console.WriteLine($"Example: $ {PROGRAM_NAME} get 'New York' 2024-05-24 12:01:17.52 ./my_table.csv");
    }

    private static void PrintError(string s) => PrintColor(s, ConsoleColor.Red);
    private static void PrintWarn(string s) => PrintColor(s, ConsoleColor.Yellow);
    private static void PrintSuggestion(string s) => PrintColor(s, ConsoleColor.Blue);
    private static void PrintFix(string s) => PrintColor(s, ConsoleColor.Green);

    private static void PrintColor(string s, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.WriteLine(s);
        Console.ResetColor();
    }

    private static void PrintWithFocusOnArgument(string[] args, int argument_index, ConsoleColor color) {
        string prefix = $"$ {PROGRAM_NAME} ";
        string focused_arg = "";
        string postfix = " ";

        foreach ( (string arg, int i) in args.Select((x, i) => (x, i)) ) {
            if (i < argument_index) {
                prefix += arg + " ";
            } else if (i == argument_index) {
                focused_arg = arg;
            } else {
                postfix += arg + " ";
            }
        }

        // Line
        Console.Write(prefix);

        Console.ForegroundColor = color;
        Console.Write(focused_arg);
        Console.ResetColor();

        Console.Write(postfix);
        Console.WriteLine("");

        // Next line ^~~~~~
        Console.Write(new string(' ', prefix.Length));
        Console.ForegroundColor = color;
        Console.Write("^");

        if (focused_arg.Length > 1)
            Console.Write(new string('~', focused_arg.Length - 1));
        Console.ResetColor();
        Console.WriteLine("");

    }
    private static void PrintWithoutFocus(string[] args) {
        Console.WriteLine($"$ {PROGRAM_NAME} {string.Join(" ", args)}");

    }
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