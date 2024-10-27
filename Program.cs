using log4net.Config;
using log4net;
using System.Reflection;
using System.Data;
using DeliveryFilter.ParseV2;

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

        var gen_mockup        = new Named("gen_mockup",        new ParseV2.FilePath("output file", "mock", ".csv"));
        var check_correctness = new Named("check_correctness", new ParseV2.FilePath("input file", "data", ".csv"));
        var fix_table         = new Named("fix_table",         new ParseV2.FilePath("input file", "data", ".csv"));
        var get               = new Named("get", 
                new ParseV2.Unordered((_, _) => new ParseResult(), 
                    [
                        new ParseV2.Unordered.Option{ Name = "--file1", ArgsCount = 1, Parser = new ParseV2.FilePath("input file", "data", ".csv") },
                        new ParseV2.Unordered.Option{ Name = "--file2", ArgsCount = 1, Parser = new ParseV2.FilePath("input file", "data", ".csv") },
                    ]
                )   
            );

        var variant = new Variant();
        variant.Add(gen_mockup);
        variant.Add(check_correctness);
        variant.Add(fix_table);
        variant.Add(get);

        string[] parse_args = args;
        ParseResult result = variant.TryParse(parse_args);

        while (result.Error != "") {
            PrintParseResultError(result, parse_args);
            PrintParseResultFix(result, parse_args);

            if (result.FixedArgs is null)
                break;

            parse_args = result.FixedArgs.Where(x => x.Length > 0).ToArray();
            result = variant.TryParse(parse_args);
        }

        return 0;
    }

    private static ParseResult CombineGet(ParseV2.Unordered.ParsedValue[] values, string args[]) {
        
    }

    private static void PrintParseResultError(ParseResult result, string[] args) {
        if (result.Error != "") {
            PrintError($"Error: {result.Error}");

            if (result.ErrorFocus.Count > 0)
                PrintWithFocusOnArgument(args, result.ErrorFocus, ConsoleColor.Red, ConsoleColor.DarkRed);
        }
    }
    private static void PrintParseResultFix(ParseResult result, string[] args) {
        if (result.FixExplanation != "") {
            PrintSuggestion($"Suggestion: {result.FixExplanation}");

            if (result.FixInputFocus.Count > 0)
                PrintWithFocusOnArgument(args, result.FixInputFocus, ConsoleColor.Blue, ConsoleColor.DarkBlue);
            
            if (result.FixedArgs is not null) {
                if (result.FixOuputFocus.Count > 0)
                    PrintWithFocusOnArgument(result.FixedArgs, result.FixOuputFocus, ConsoleColor.Green, ConsoleColor.DarkGreen);
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

    struct Segment {
        public string Text;
        public bool IsColored;
    };
    private static void PrintWithFocusOnArgument(string[] args, IEnumerable<uint> focus, ConsoleColor color, ConsoleColor color_bg) {
        List<Segment> segments = new();

        segments.Add(new Segment() {
            Text = $"$ {PROGRAM_NAME}",
            IsColored = false,
        });

        uint index = 0;
        foreach (string arg_in in args) {
            string arg = arg_in;
            if (arg.Contains(' ')) {
                arg = $"\"{arg}\"";
            }

            Segment s = new() {
                Text = arg,
                IsColored = focus.Contains(index),
            };

            if (s.IsColored && s.Text.Length == 0)
                s.Text = " ";

            segments.Add(s);
            index++;
        }

        // Line
        foreach (Segment s in segments) {
            if (s.IsColored) {
                Console.ForegroundColor = color;
                if (s.Text == " ")
                    Console.BackgroundColor = color_bg;
            }
            Console.Write(s.Text);
            Console.ResetColor();
            Console.Write(" ");
        }
        Console.WriteLine();

        // Next line ^~~~~~
        foreach (Segment s in segments) {
            if (s.IsColored) {
                Console.ForegroundColor = color;
                Console.Write("^");
                if (s.Text.Length > 1)
                    Console.Write(new string('~', s.Text.Length - 1));
                Console.ResetColor();
            } else {
                Console.Write(new string(' ', s.Text.Length));
            }
            Console.Write(" ");
        }
        Console.WriteLine();
    }
    private static void PrintWithoutFocus(string[] args) {
        Console.Write($"$ {PROGRAM_NAME}");

        Console.ForegroundColor = ConsoleColor.Green;
        foreach (string arg_in in args) {
            string arg = arg_in;
            if (arg.Contains(' ')) {
                arg = $"\"{arg}\"";
            }
            
            Console.Write(arg);
            Console.Write(" ");
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    private static string EscapeQuotes(string input) {
        return input.Replace("\"", "\\\"").Replace("'", "\\'");
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