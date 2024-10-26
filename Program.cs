using log4net.Config;
using log4net;
using System.Reflection;

namespace DeliveryFilter;

internal class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));

    static int Main(string[] args)
    {
        if (Assembly.GetEntryAssembly() is Assembly asm) {
            var logRepository = LogManager.GetRepository(asm);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
        log.Info("Program starts");

        if (args.Length < 1) {
            Console.WriteLine("Invalid usage");
        }

        string arg = args[0];
        Console.WriteLine(arg);
        Console.WriteLine("");

        ParseReport rep;
        try {
            rep = DateParser.ParseDate(arg);
        } catch (ArgumentException exp) {
            Console.WriteLine(exp);
            return 1;
        }

        if (rep.Errors.Length > 0 || rep.FixExplanation.Length > 0) {
            Console.Write($"Where: `");
            Console.ForegroundColor = rep.Errors.Length > 0 ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.Write($"{arg}");
            Console.ResetColor();
            Console.Write($"`\n");

            Console.WriteLine($"        ^~~~~~~~~~");

            Console.Write($"Fix:   `");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{rep.FixSuggestion}");
            Console.ResetColor();
            Console.Write($"`\n");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        foreach (string err in rep.Errors.Split(";")) {
            string msg = err.Trim();
            if (msg.Length > 0)
                Console.WriteLine(msg);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (string warn in rep.FixExplanation.Split(";")) {
            string msg = warn.Trim();
            if (msg.Length > 0)
                Console.WriteLine(msg);
        }
        Console.ResetColor();


        if (rep.Date is null) {
            Console.WriteLine("Failed to parse.");
            return 2;
        }

        Console.WriteLine(rep.Date);
        return 0;
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