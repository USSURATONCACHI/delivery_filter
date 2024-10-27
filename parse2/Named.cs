namespace DeliveryFilter.ParseV2;

public class Named(string name, IParser? next) : IParser {
    readonly string Name = name;
    readonly IParser? Next = next;

    public ParseResult TryParse(string[] args)
    {
        // Nothing provided
        if (args.Length == 0) {
            return new ParseResult() {
                HeuristicScore = Double.MaxValue,

                Error = "No command provided.",
                ErrorFocus = [0],

                FixExplanation = $"Use any command, for example: {Name}.",
                FixedArgs = [Name],
                FixInputFocus = [0],
                FixOuputFocus = [0],
            };
        }

        // Exact match
        if (args[0] == Name) {
            if (Next is not null)
                return Next.TryParse(args[1..])
                           .Prepend(args[..1], 0.0);
            else
                return new ParseResult(null);
        }

        // Does not match
        ParseResult result = new()
        {
            Error = "Incorrect command specified",
            ErrorFocus = [0],
        };

        // Find argument with lowest distance
        (string arg, int index, double lev_dist) = args
            .Select(  
                (arg, index) => (arg, index, (double) Fastenshtein.Levenshtein.Distance(arg, Name) / Name.Length)  
            )
            .OrderBy(x => x.Item3)
            .First();

        result.HeuristicScore += 1.0 + lev_dist;
        if (lev_dist < 1.0) {
            // Close enough
            string closest_command_text = args[index];

            if (closest_command_text == Name)
                result.FixExplanation = $"Take '{Name}'";
            else
                result.FixExplanation = $"Replace '{closest_command_text}' with '{Name}'";

            if (index != 0) {
                result.FixInputFocus = [(uint) index];
                result.FixExplanation += " and move it to the first argument";
            }
            result.FixExplanation += ".";

            var args_list = args.ToList();
            args_list.RemoveAt(index);
            args_list.Insert(0, Name);

            result.FixedArgs = args_list.ToArray();
            result.FixOuputFocus = [0];
        } else {
            // No suitable candidate
            result.FixExplanation = $"Put `{Name}` as your first argument.";

            var args_list = args.ToList();
            args_list.Insert(0, Name);
            result.FixedArgs = args_list.ToArray();
            result.FixOuputFocus = [0];
        }

        return result;
    }
}
