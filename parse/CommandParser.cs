namespace DeliveryFilter.Parse;

public enum CommandType {
    Get,
    FixTable,
    CheckCorrectness,
    GenMockup,
    Help,
};

public class CommandTypeParser {
    public static (ParseReport<CommandType>, double) ParseCommandType(string s) {
        s = s.ToLower();

        (string, CommandType)[] commands = [
            ("get",               CommandType.Get),
            ("fix_table",         CommandType.FixTable),
            ("check_correctness", CommandType.CheckCorrectness),
            ("gen_mockup",        CommandType.GenMockup),
            ("help",              CommandType.Help),
        ];

        // Check for exact match
        foreach ((string com_text, CommandType command) in commands) {
            if (s == com_text)
                return (new ParseReport<CommandType>(command, s, "", ""), 0);
        }

        // Check the closest match
        Fastenshtein.Levenshtein lev = new(s);
        (double dist, string closest, CommandType closest_command) = commands
            .Select(
                elem => ((double) lev.DistanceFrom(elem.Item1) / s.Length, elem.Item1, elem.Item2)
            )
            .OrderBy(elem => elem.Item1)
            .First();

        // Return error
        string errs = $"No suitable command found. You may have meant `{closest}`; ";
        return (new ParseReport<CommandType>(closest_command, closest, "", errs), dist);
    }
}