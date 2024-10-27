namespace DeliveryFilter.Parse;

struct HelpArgs {
}

struct CheckCorrectnessArgs {
    public string Filepath;
}

struct GenMockupArgs {
    public string Filepath;
}

struct FixTableArgs {
    public string Filepath;
}

struct GetArgs {
    public string Filepath;
    public string District;
    public DateTime StartTime;
    bool IsJson;
}


public class ArgsParser {
    public struct ParseResult {
        public object? Parsed;

        public string Error;
        public int ErrorFocus;


        public string FixExplanation;
        public string[]? FixedArgs;
        public int FixInputFocus;
        public int FixOuputFocus;


        public ParseResult() {
            Parsed = null;

            Error = "";
            ErrorFocus = -1;

            FixExplanation = "";
            FixedArgs = null;
            FixInputFocus = -1;
            FixOuputFocus = -1;
        }

        public ParseResult(object? o) : this() { 
            Parsed = o;
        }

        public ParseResult Prepend(string[] prepend_args) => new() {
            Parsed = this.Parsed,

            Error = this.Error,
            ErrorFocus = PrependFocusIndex(this.ErrorFocus, prepend_args.Length),

            FixExplanation = this.FixExplanation,
            FixedArgs = prepend_args.Concat(this.FixedArgs).ToArray(),
            FixInputFocus = PrependFocusIndex(this.FixInputFocus, prepend_args.Length),
            FixOuputFocus = PrependFocusIndex(this.FixOuputFocus, prepend_args.Length),
        };

        private static int PrependFocusIndex(int old_one, int add) => old_one == -1 ? -1 : (old_one + add);
    }

    public static ParseResult ParseAndCheck(string[] args) {
        // Check that there is at least one argument
        if (args.Length == 0) {
            return new ParseResult() 
            {
                Error = "No command specified",

                FixExplanation = "Try to use a command, like `help`",
                FixedArgs = ["help"],
                FixOuputFocus = 0,
            };
        } else {
            return ParseAndCheckCommandType(args);
        }
    }

    private static ParseResult ParseAndCheckCommandType(string[] args) {
        (var command, int levenshtein_distance) = CommandTypeParser.ParseCommandType(args[0]);
        bool is_command = command.Errors.Length == 0;

        if (command.Errors.Length == 0) {
            if (!command.Value.HasValue)
                throw new Exception("Internal error at parsing command type");
            
            return ParseAndCheckCommand(command.Value.Value, args[1..]).Prepend(args[..1]);
        }
        ParseResult result = new()
        {
            Error = "Incorrect command specified",
            ErrorFocus = 0,
        };

        // Find argument with lowest distance
        int closest_command_dist = Int32.MaxValue;
        int closest_command_index = -1;
        string closest_command_suggestion = "";

        foreach ((string arg, int index) in args.Select((x, index) => (x, index))) {
            if (DoesLookLikeDate(arg) || DoesLookLikeTime(arg))
                continue;
            
            // Command
            bool looks_like_command = false;
            (var command_rep, int lev_dist) = CommandTypeParser.ParseCommandType(arg);
            looks_like_command = lev_dist < command_rep.FixSuggestion.Length;

            if (looks_like_command && lev_dist < closest_command_dist) {
                closest_command_dist = lev_dist;
                closest_command_index = index;
                closest_command_suggestion = command_rep.FixSuggestion;
            }
        }

        if (closest_command_index != -1) {
            // Suggest fixing that argument
            
            string closest_command_text = args[closest_command_index];

            if (closest_command_index != 0)
                result.FixExplanation = $"Replace '{closest_command_text}' with '{closest_command_suggestion}' and move it to the first argument";
            else
                result.FixExplanation = $"Replace '{closest_command_text}' with '{closest_command_suggestion}'";


            var args_list = args.ToList();
            args_list.RemoveAt(closest_command_index);
            args_list.Insert(0, closest_command_suggestion);


            result.FixedArgs = args_list.ToArray();
            result.FixInputFocus = closest_command_index;
            result.FixOuputFocus = 0;
        } else {
            // Suggest adding a command
            result.FixExplanation = "Put `get` as your first argument";

            var args_list = args.ToList();
            args_list.Insert(0, "get");
            result.FixedArgs = args_list.ToArray();
            result.FixOuputFocus = 0;
        }

        return result;
    }

    private static ParseResult ParseAndCheckCommand(CommandType type, string[] args) {
        switch (type) {
            case CommandType.Help:
                return new ParseResult(new HelpArgs()) {
                    FixExplanation = "Unneccessary arguments after 'help'",
                    FixedArgs = [],
                    FixInputFocus = 0,
                    FixOuputFocus = -1,
                };

            case CommandType.Get:
            case CommandType.CheckCorrectness:
            case CommandType.FixTable:
            case CommandType.GenMockup:
            default:
                throw new NotImplementedException();
        }
    }

    private static bool DoesLookLikeTime(string s) {
        try {
            TimeParser.ParseTime(s);
            return true;
        } catch (Exception) {
            return false;
        }
    }
    private static bool DoesLookLikeDate(string s) {
        try {
            DateParser.ParseDate(s);
            return true;
        } catch (Exception) {
            return false;
        }
    }
}