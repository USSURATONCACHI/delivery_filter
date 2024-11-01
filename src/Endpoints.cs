using log4net;
using Csv;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Policy;
using log4net.Config;

namespace DeliveryFilter;

public class Endpoints {
    private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));
    private static readonly string[] FORMATS = new[] { 
                    "yyyy-MM-dd HH:mm:ss", 
                    "yyyy-MM-dd HH:mm", 
                    "yyyy-MM-dd"
                };

    public Endpoints() {

    }


    public void FixTable(string file) {

    }

    public void CheckCorrectness(string file) {

    }

    public void GetMockup(string file) {

    }

    public void Get(string district, DateTime datetime, string file) {
        LOG.Info($"Input datetime is: {datetime}");

        try {
            var entries = ReadTableToFile(file)
                .Where(d => d.DeliveryTime >= datetime);

            if (district is not null)
                entries = entries.Where(d => d.District.Trim().Equals(district.Trim(), StringComparison.CurrentCultureIgnoreCase));
            
            foreach (var entry in entries) {
                LOG.Info($"Entry: {entry.Id} {entry.District} {entry.DeliveryTime} {entry.Weight}");
            }
        } catch (FileNotFoundException) {
            LogError($"File '{file}' not found");
        }
    }

    protected static void LogError(string text) {
        LOG.Error(text);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    protected static void LogWarn(string text) {
        LOG.Warn(text);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    protected static IEnumerable<Delivery> ReadTableToFile(string filepath) {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);

        bool checked_columns = false;
        foreach (var line in CsvReader.Read(reader)) {
            if (!checked_columns) {
                if (!CheckColumns(line))
                    throw new Exception("CSV does not have required columns");
                checked_columns = true;
            }

            if (TryReadDelivery(line, out Delivery d))
                yield return d;
            else
                LOG.Info("Skipping 1 row due to failure");
        }
    }

    protected static bool CheckColumns(ICsvLine line) {
        bool result = true;

        if (!line.HasColumn("Id")) {
            LogError("Data file does not have `Id` column");
            result = false;
        }

        if (!line.HasColumn("Datetime")) {
            LogError("Data file does not have `Datetime` column");
            result = false;
        }

        if (!line.HasColumn("Weight")) {
            LogError("Data file does not have `Weight` column");
            result = false;
        }

        if (!line.HasColumn("District")) {
            LogError("Data file does not have `District` column");
            result = false;
        }

        return result;
    }

    protected static bool TryReadDelivery(ICsvLine line, out Delivery d) {
        string full_row = string.Join(' ', line.Raw);
        d = new();

        if (!long.TryParse(line["Id"], out d.Id))
            LogWarn($"Cannot parse ID '{line["Id"]}' for line '{full_row}'.");

        d.District = line["District"];
        bool datetime_parse = DateTime.TryParseExact(line["Datetime"], FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out d.DeliveryTime);
        if (!datetime_parse) {
            LogError($"Cannot parse delivery datetime '{line["Datetime"]}' for line '{full_row}'");
            return false;
        }

        if (!double.TryParse(line["Weight"], out d.Weight))
            LogWarn($"Value '{line["Weight"]}' is not a number for line '{full_row}'.");

        if (d.Weight < 0)
            LogWarn($"Delivery ID ${d.Id} weight is negative for line '{full_row}'.");

        return true;
    }
}