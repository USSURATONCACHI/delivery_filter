namespace DeliveryFilter.ParseV2;

public struct ParseResult {
    public object? Parsed;
    public double HeuristicScore; // lower = better

    public string Error;
    public List<uint> ErrorFocus;

    public string FixExplanation;
    public string[]? FixedArgs;

    public List<uint> FixInputFocus;
    public List<uint> FixOuputFocus;


    public ParseResult() {
        Parsed = null;
        HeuristicScore = 1.0;

        Error = "";
        ErrorFocus = [];

        FixExplanation = "";
        FixedArgs = null;
        FixInputFocus = [];
        FixOuputFocus = [];
    }
    
    public ParseResult(object? o) : this() { 
        Parsed = o;
    }

    public ParseResult Prepend(string[] prepend_args, double score_add) => new() {
        Parsed = this.Parsed,
        HeuristicScore = this.HeuristicScore + score_add,

        Error = this.Error,
        ErrorFocus = PrependFocus(this.ErrorFocus, (uint)prepend_args.Length),

        FixExplanation = this.FixExplanation,
        FixedArgs = this.FixedArgs is null ? prepend_args : [.. prepend_args, .. this.FixedArgs],
        FixInputFocus = PrependFocus(this.FixInputFocus, (uint)prepend_args.Length),
        FixOuputFocus = PrependFocus(this.FixOuputFocus, (uint)prepend_args.Length),
    };

    public ParseResult Postpend(string[] additional_args) => new() {
        Parsed = this.Parsed,
        HeuristicScore = this.HeuristicScore,

        Error = this.Error,
        ErrorFocus = this.ErrorFocus,

        FixExplanation = this.FixExplanation,
        FixedArgs = this.FixedArgs is null ? additional_args : [.. this.FixedArgs, .. additional_args],
        FixInputFocus = this.FixInputFocus,
        FixOuputFocus = this.FixOuputFocus,
    };

    private static List<uint> PrependFocus(List<uint> focus, uint add) => 
        focus.Select(x => x + add).Order().ToList();
}
