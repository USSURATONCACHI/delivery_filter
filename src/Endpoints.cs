using log4net;
using Csv;
using System.Globalization;

namespace DeliveryFilter;

public class Endpoints {
    private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));
    private static readonly string[] FORMATS = new[] { 
                    "yyyy-MM-dd HH:mm:ss", 
                    "yyyy-MM-dd HH:mm", 
                    "yyyy-MM-dd HH", 
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

    public void Get(string district, DateTime datetime, bool json, string file) {
        LOG.Info($"Input datetime is: {datetime}");
        var entries = ReadTableToFile(file)
            .Where(d => d.DeliveryTime >= datetime);

        if (district is not null)
            entries = entries.Where(d => d.District.Trim().Equals(district.Trim(), StringComparison.CurrentCultureIgnoreCase));
        
        
        foreach (var entry in entries) {
            LOG.Info($"Entry: {entry.Id} {entry.District} {entry.DeliveryTime} {entry.Weight}");
        }
    }

    protected static IEnumerable<Delivery> ReadTableToFile(string filepath) {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);

        foreach (var line in CsvReader.Read(reader))
            yield return ReadRowToDelivery(line);
    }

    protected static Delivery ReadRowToDelivery(ICsvLine line) {
        return new Delivery
        {
            Id = long.Parse(line["Id"]),
            District = line["District"],
            DeliveryTime = DateTime.ParseExact(
                line["Datetime"], 
                FORMATS, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None
            ),
            Weight = double.Parse(line["Weight"])
        };
    }
}