namespace DeliveryFilter.ParseV2;

public interface IParser {
    public ParseResult TryParse(string[] args);
}
