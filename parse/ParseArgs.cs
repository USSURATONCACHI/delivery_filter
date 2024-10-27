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
            FixedArgs = this.FixedArgs is null ? prepend_args : [.. prepend_args, .. this.FixedArgs],
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
        (var command, double levenshtein_distance) = CommandTypeParser.ParseCommandType(args[0]);
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
        double closest_command_dist = Int32.MaxValue;
        int closest_command_index = -1;
        string closest_command_suggestion = "";

        foreach ((string arg, int index) in args.Select((x, index) => (x, index))) {
            if (DoesLookLikeDate(arg) || DoesLookLikeTime(arg))
                continue;
            
            // Command
            bool looks_like_command = false;
            (var command_rep, double lev_dist) = CommandTypeParser.ParseCommandType(arg);
            looks_like_command = lev_dist < 1.0;

            if (looks_like_command && lev_dist < closest_command_dist) {
                closest_command_dist = lev_dist;
                closest_command_index = index;
                closest_command_suggestion = command_rep.FixSuggestion;
            }
        }

        if (closest_command_index != -1) {
            // Suggest fixing that argument
            
            string closest_command_text = args[closest_command_index];

            if (closest_command_text == closest_command_suggestion)
                result.FixExplanation = $"Take '{closest_command_suggestion}'";
            else
                result.FixExplanation = $"Replace '{closest_command_text}' with '{closest_command_suggestion}'";

            if (closest_command_index != 0) {
                result.FixInputFocus = closest_command_index;
                result.FixExplanation += " and move it to the first argument";
            }

            var args_list = args.ToList();
            args_list.RemoveAt(closest_command_index);
            args_list.Insert(0, closest_command_suggestion);


            result.FixedArgs = args_list.ToArray();
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
            case CommandType.Help: {
                if (args.Length > 0) {
                    return new ParseResult(new HelpArgs()) {
                        Error = "Unneccessary arguments after 'help'",
                        ErrorFocus = 0,

                        FixExplanation = "Remove unneccessary arguments",
                        FixedArgs = [],
                        FixOuputFocus = 0,
                    };
                } else {
                    return new ParseResult(new HelpArgs());
                }
            }

            case CommandType.GenMockup:        return ParseAndCheckGenMockup(args);
            case CommandType.CheckCorrectness: return ParseAndCheckCheckCorrectness(args);
            case CommandType.FixTable:         return ParseAndCheckFixTable(args);

            case CommandType.Get:
            default:
                throw new NotImplementedException();
        }
    }


    struct FilepathParsingParams {
        public string NoFileErrorText;
        public string DefaultFileName;
        public string DefaultFileExtension;

    }
    private static ParseResult ParseAndCheckGenMockup(string[] args) {
        FilepathParsingParams paramss = new() {
            NoFileErrorText = "No output file specified.",
            DefaultFileName = "mock",
            DefaultFileExtension = ".csv",
        };
        
        return ParseAndCheckFilepath(args, paramss, filepath => new GenMockupArgs() { Filepath = filepath });
    }

    private static ParseResult ParseAndCheckCheckCorrectness(string[] args) {
        FilepathParsingParams paramss = new() {
            NoFileErrorText = "No input file specified.",
            DefaultFileName = "data",
            DefaultFileExtension = ".csv",
        };
        
        return ParseAndCheckFilepath(args, paramss, filepath => new CheckCorrectnessArgs() { Filepath = filepath });
    }
    private static ParseResult ParseAndCheckFixTable(string[] args) {
        FilepathParsingParams paramss = new() {
            NoFileErrorText = "No input file specified.",
            DefaultFileName = "data",
            DefaultFileExtension = ".csv",
        };
        
        return ParseAndCheckFilepath(args, paramss, filepath => new FixTableArgs() { Filepath = filepath });
    }

    private static ParseResult ParseAndCheckFilepath(string[] args, FilepathParsingParams paramss, Func<string, object> func) {
        if (args.Length < 1) {
            return new ParseResult() {
                Error = paramss.NoFileErrorText,

                FixExplanation = "Add a filepath",
                FixedArgs = [$"./{paramss.DefaultFileName}{paramss.DefaultFileExtension}"],
                FixOuputFocus = 0,
            };
        }

        string filepath = args[0];

        if (!IsValidPath(filepath)) {
            string suggested_path = RemoveInvalidPathChars(filepath);
            if (suggested_path.EndsWith('/') || suggested_path.EndsWith('\\'))
                suggested_path += paramss.DefaultFileName;

            if (suggested_path == "")
                suggested_path += "./" + paramss.DefaultFileName;

            if (!suggested_path.EndsWith(paramss.DefaultFileExtension))
                suggested_path += paramss.DefaultFileExtension;

            
            return new ParseResult() {
                Error = $"'{filepath}' is not a valid filepath.",
                ErrorFocus = 0,

                FixExplanation = "Remove invalid characters from path.",
                FixedArgs = (new[]{suggested_path}).Concat(args[1..]).ToArray(),
                FixOuputFocus = 0,
            };
        }

        if (filepath.EndsWith('/') || filepath.EndsWith('\\')) {
            return new ParseResult() {
                Error = $"'{filepath}' is not a filepath.",
                ErrorFocus = 0,

                FixExplanation = "Add a filename.",
                FixedArgs =  (new[]{filepath + "mock.csv"}).Concat(args[1..]).ToArray(),
                FixOuputFocus = 0,
            };
        }

        ParseResult result = new(func(filepath));

        if (args.Length > 1) {
            result.Error = "Unneccessary arguments in the end.";
            result.ErrorFocus = 1;

            result.FixedArgs = args[..1];
            result.FixExplanation = "Remove unneccessary arguments from path.";
            result.FixOuputFocus = 1;
        }

        return result;
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
    private static bool IsValidPath(string path) {
        char[] invalidChars = Path.GetInvalidPathChars();
        return !path.Any(ch => invalidChars.Contains(ch));
    }

    private static string RemoveInvalidPathChars(string path) {
        char[] invalidChars = Path.GetInvalidPathChars();
        return new string(path.Where(ch => !invalidChars.Contains(ch)).ToArray());
    }

}