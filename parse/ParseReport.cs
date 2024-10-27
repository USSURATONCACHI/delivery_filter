namespace DeliveryFilter.Parse;

public struct ParseReport<T> where T : struct {
    public T? Value;

    public string FixSuggestion;
    public string Warns;
    public string Errors;

    public ParseReport(T? value, string fix_suggestion, string warns, string errors) {
        Value = value;
        FixSuggestion = fix_suggestion;
        Warns = warns;
        Errors = errors;
    }
}

