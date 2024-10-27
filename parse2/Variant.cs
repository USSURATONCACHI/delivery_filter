namespace DeliveryFilter.ParseV2;

public class Variant : IParser {
    List<IParser> Variants;

    public Variant() {
        Variants = new();
    }

    public void Add(IParser parser) => Variants.Add(parser);

    public ParseResult TryParse(string[] args)
    {
        ParseResult[] results = Variants
            .Select(v => v.TryParse(args))
            .OrderBy(result => result.HeuristicScore)
            .ToArray();

        if (results.Length == 0)
            throw new Exception("No parsers added");

        return results.First();
    }
}
