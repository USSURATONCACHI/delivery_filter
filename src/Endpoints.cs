using log4net;
using Csv;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Policy;
using log4net.Config;
using System.Runtime.Serialization;

namespace DeliveryFilter;

public class Endpoints {
    private static ILog LOG = LogManager.GetLogger(typeof(Endpoints));
    private static readonly string[] FORMATS = new[] { 
                    "yyyy-MM-dd HH:mm:ss", 
                    "yyyy-MM-dd HH:mm", 
                    "yyyy-MM-dd"
                };

    public Endpoints() {}

    public void Get(string district, DateTime start_datetime, DateTime end_datetime, string logfile, string outfile, string file) {
        if (logfile is not null)
            LogUtil.AddFileLog4Net(logfile);

        LOG.Info($"Input file is: {file}");
        LOG.Info($"Input datetime is: {start_datetime} to {end_datetime}");

        if (outfile is null)
            LogUtil.LogWarn(LOG, "Output file is not set");
        else
            LOG.Info($"Output file is: {outfile}");

        try {
            var entries = ReadTableToFile(file)
                .Where(d => d.DeliveryTime >= start_datetime)
                .Where(d => d.DeliveryTime <= end_datetime);

            if (outfile == file)
                entries = entries.ToArray(); // We cannot stream from file to itself, so read to RAM entriely first

            if (district is not null)
                entries = entries.Where(d => d.District.Trim().Equals(district.Trim(), StringComparison.CurrentCultureIgnoreCase));

            if (outfile is not null)
                WriteToFileCsv(entries, outfile);
        } catch (FileNotFoundException) {
            LogUtil.LogError(LOG, $"File '{file}' not found");
        } catch (NoRequiredColumnsException) {
            LogUtil.LogError(LOG, $"CSV does not have required columns");
        }
    }

    protected static void WriteToFileCsv(IEnumerable<Delivery> entries, string filepath) {
        using (var writer = new StreamWriter(filepath)) {
            writer.WriteLine("Id,District,Datetime,Weight"); // CSV header
            foreach (var entry in entries) {
                LOG.Info($"Entry: {entry.Id} {entry.District} {entry.DeliveryTime} {entry.Weight}");

                writer.WriteLine($"{entry.Id},{entry.District},{entry.DeliveryTime:yyyy-MM-dd HH:mm:ss},{entry.Weight}");
            }
        }
    }


    protected static IEnumerable<Delivery> ReadTableToFile(string filepath) {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);

        bool checked_columns = false;
        foreach (var line in CsvReader.Read(reader)) {
            if (!checked_columns) {
                if (!CheckColumns(line))
                    throw new NoRequiredColumnsException();
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
            LogUtil.LogError(LOG, "Data file does not have `Id` column");
            result = false;
        }

        if (!line.HasColumn("Datetime")) {
            LogUtil.LogError(LOG, "Data file does not have `Datetime` column");
            result = false;
        }

        if (!line.HasColumn("Weight")) {
            LogUtil.LogError(LOG, "Data file does not have `Weight` column");
            result = false;
        }

        if (!line.HasColumn("District")) {
            LogUtil.LogError(LOG, "Data file does not have `District` column");
            result = false;
        }

        return result;
    }

    protected static bool TryReadDelivery(ICsvLine line, out Delivery d) {
        string full_row = string.Join(' ', line.Raw);
        d = new();

        if (!long.TryParse(line["Id"], out d.Id))
            LogUtil.LogWarn(LOG, $"Cannot parse ID '{line["Id"]}' for line '{full_row}'.");

        d.District = line["District"];
        bool datetime_parse = DateTime.TryParseExact(line["Datetime"], FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out d.DeliveryTime);
        if (!datetime_parse) {
            LogUtil.LogError(LOG, $"Cannot parse delivery datetime '{line["Datetime"]}' for line '{full_row}'");
            return false;
        }

        if (!double.TryParse(line["Weight"], out d.Weight))
            LogUtil.LogWarn(LOG, $"Value '{line["Weight"]}' is not a number for line '{full_row}'.");

        if (d.Weight < 0)
            LogUtil.LogWarn(LOG, $"Delivery ID ${d.Id} weight is negative for line '{full_row}'.");

        return true;
    }
}