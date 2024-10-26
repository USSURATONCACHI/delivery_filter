namespace DeliveryFilter;

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
    ArgsParser()
    {

    }

    public object ParseArgs(string[] argv) {
        return null;
    }

    private void ParseCommand(string command) {
        if (command == "gen_mockup") {

        } else if (command == "check_correctness") {

        } else if (command == "fix_table") {

        } else if (command == "get") {
            
        }
    }
}