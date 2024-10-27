namespace DeliveryFilter.ParseV2;

public class Unordered : IParser {
    public struct Option {
        public string Name;
        public uint ArgsCount;
        public IParser Parser;
    }
    public struct ParsedValue {
        public string Name;
        public object? Value;
    }

    List<Option> Options;
    Func<ParsedValue[], string[], ParseResult> Combine;

    public Unordered(Func<ParsedValue[], string[], ParseResult> combine) {
        Options = new();
        Combine = combine;
    }

    public Unordered(Func<ParsedValue[], string[], ParseResult> combine, IEnumerable<Option> options) {
        Options = new();
        Combine = combine;
        Options.AddRange(options);
    }

    public void Add(string name, uint args_count, IParser parser) => Options.Add(new Option() {
        Name = name,
        ArgsCount = args_count,
        Parser = parser,
    });

    public ParseResult TryParse(string[] args)
    {
        List<ParsedValue> values = new();

        for (uint i = 0; i < args.Length; i++) {
            string arg = args[i];

            // Check if we have option with such name
            ParsedValue? value = null;
            foreach (Option opt in Options) {
                if (opt.Name != arg) 
                    continue;
                
                uint start_id = i + 1;
                uint end_id = uint.Min(start_id + opt.ArgsCount, (uint)args.Length);

                ParseResult result = opt.Parser.TryParse(args[(int)start_id..(int)end_id]);
                if (result.Error != "") {
                    return result
                        .Prepend(args[..(int)start_id], 0.0)
                        .Postpend(args[(int)end_id..]);
                }

                value = new ParsedValue() {
                    Name = arg,
                    Value = result.Parsed,
                };
                i = end_id - 1;
                break;
            }

            if (value is not null) {
                values.Add(value.Value);
                continue;
            }

            // Unnamed value
            if (Options.Where(x => x.Name == "").Count() == 0) {
                return new ParseResult() {
                    ErrorFocus = [i],
                    Error = "Unknown unnamed parameter.",

                    FixedArgs = args.Where( (_, index) => index != i).ToArray(),
                    FixExplanation = "Remove this parameter.",
                    FixOuputFocus = i > 0 ? [i - 1] : [],
                };
            }

            ParseResult parsed = Options
                .Where(x => x.Name == "")
                .Select(x => x.Parser.TryParse([ args[i] ]))
                .OrderBy(r => r.HeuristicScore)
                .First();

            if (parsed.Error != "") {
                return parsed
                    .Prepend(args[..(int)i], 0.0)
                    .Postpend(args[(int)(i + 1)..]);
            }

            values.Add(new ParsedValue() {
                Name = "",
                Value = parsed.Parsed,
            });
        }

        // Combine
        return Combine(values.ToArray(), args);
    }
}
