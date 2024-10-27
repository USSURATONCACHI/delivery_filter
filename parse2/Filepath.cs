namespace DeliveryFilter.ParseV2;

public class FilePath : IParser {
    string Hint;
    string DefaultFileName;
    string DefaultFileExtension;

    public FilePath(string hint, string default_file_name, string default_extension) {
        Hint = hint;
        DefaultFileName = default_file_name;
        DefaultFileExtension = default_extension;
    }

    public ParseResult TryParse(string[] args)
    {
        if (args.Length < 1) {
            return new ParseResult() {
                Error = $"No {Hint} specified.",
                ErrorFocus = [0],

                FixExplanation = "Add a filepath.",
                FixedArgs = [$"./{DefaultFileName}{DefaultFileExtension}"],
                FixOuputFocus = [0],
            };
        }

        string filepath = args[0];

        if (!IsValidPath(filepath)) {
            string suggested_path = RemoveInvalidPathChars(filepath);
            if (suggested_path.EndsWith('/') || suggested_path.EndsWith('\\'))
                suggested_path += DefaultFileName;

            if (suggested_path == "")
                suggested_path += "./" + DefaultFileName;

            if (!suggested_path.EndsWith(DefaultFileExtension))
                suggested_path += DefaultFileExtension;

            
            return new ParseResult() {
                Error = $"'{filepath}' is not a valid filepath.",
                ErrorFocus = [0],

                FixExplanation = "Remove invalid characters from path.",
                FixedArgs = (new[]{suggested_path}).Concat(args[1..]).ToArray(),
                FixOuputFocus = [0],
            };
        }

        if (filepath.EndsWith('/') || filepath.EndsWith('\\')) {
            return new ParseResult() {
                Error = $"'{filepath}' is not a filepath.",
                ErrorFocus = [0],

                FixExplanation = "Add a filename.",
                FixedArgs =  (new[]{filepath + DefaultFileName + DefaultFileExtension}).Concat(args[1..]).ToArray(),
                FixOuputFocus = [0],
            };
        }

        ParseResult result = new(filepath);

        if (args.Length > 1) {
            result.Error = "Unneccessary arguments in the end.";
            result.ErrorFocus = [1];

            result.FixedArgs = args[..1];
            result.FixExplanation = "Remove unneccessary arguments.";
            result.FixOuputFocus = [1];
        }

        return result;
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
