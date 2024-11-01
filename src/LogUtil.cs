using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System.Reflection;



namespace DeliveryFilter;

public class LogUtil {
    public static void LogError(ILog log, string text) {
        log.Error(text);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void LogWarn(ILog log, string text) {
        log.Warn(text);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    public static void AddFileLog4Net(string logFileName)
    {
        if (Assembly.GetEntryAssembly() is not Assembly asm) {
            return;
        }

        var hierarchy = (Hierarchy)LogManager.GetRepository(asm);

        var layout = new log4net.Layout.PatternLayout
        {
            ConversionPattern = "%date [%level] %logger - %message%newline"
        };

        var fileAppender = new FileAppender
        {
            Name = logFileName,
            File = logFileName,
            AppendToFile = true,
            Layout = layout,
            ImmediateFlush = true
        };

        layout.ActivateOptions();
        fileAppender.ActivateOptions();
        hierarchy.Root.AddAppender(fileAppender);
    }

}