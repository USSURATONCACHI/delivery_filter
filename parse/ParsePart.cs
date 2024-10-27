namespace DeliveryFilter.Parse;

struct ParsePart<T> {
    public string TextShouldBe;
    public string Text;
    public T Value;

    public ParsePart(string value, T parsed_value) {
        Value = parsed_value;
        Text = value;
        TextShouldBe = Text;
    }
}