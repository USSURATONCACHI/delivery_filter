using Csv;

namespace DeliveryFilter;

public class Endpoints {
    public Endpoints() {

    }


    public void FixTable(string file) {

    }

    public void CheckCorrectness(string file) {

    }

    public void GetMockup(string file) {

    }

    public void Get(string district, DateTime datetime, bool json, string file) {
        foreach (var entry in ReadTableToFile(file)) {
            Console.WriteLine($"Entry: {entry.Id} {entry.District} {entry.DeliveryTime} {entry.Weight}");
        }
    }

    protected static IEnumerable<Delivery> ReadTableToFile(string filepath) {
        return ReadTableToFile(filepath, _ => true);
    }
    protected static IEnumerable<Delivery> ReadTableToFile(string filepath, Func<Delivery, bool> filter) {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);

        foreach (var line in CsvReader.Read(reader)) {
            Delivery d = ReadRowToDelivery(line);
            if (filter(d)) {
                yield return d;
            }
        }
    }

    protected static Delivery ReadRowToDelivery(ICsvLine line) {
        return new Delivery
        {
            Id = long.Parse(line["Id"]),
            District = line["District"],
            DeliveryTime = DateTime.Parse(line["Datetime"]),
            Weight = double.Parse(line["Weight"])
        };
    }
}