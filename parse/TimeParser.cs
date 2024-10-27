namespace DeliveryFilter.Parse;

public class TimeParser
{
    
    public static ParseReport<TimeOnly> ParseTime(string s) {
        // Check structure (A:B:C or A:B)
        string[] split = s.Split("-");
        if (split.Length != 3 && split.Length != 2) 
            throw new ArgumentException("Not a time (1)");

        // Parse each part
        string warns = "";
        string errs = "";

        (string e, string w) = ("", "");

        // Hours
        (ParsePart<int> hour, e, w) = ParseHour(split[0]);
        errs += e;
        warns += w;

        // Minutes
        (ParsePart<int> mins, e, w) = ParseMinutes(split[1]);
        errs += e;
        warns += w;

        // Seconds
        ParsePart<double> secs;
        if (split.Length >= 3) {
            (secs, e, w) = ParseSeconds(split[3]);
            errs += e;
            warns += w;
        } else {
            secs = new ParsePart<double>("00", 0.0);
        }

        // Return result
        string fix_suggestion = $"{hour.TextShouldBe}:{mins.TextShouldBe}:{secs.TextShouldBe}";

        if (errs.Length == 0) {
            int secs_whole = (int) Math.Floor(secs.Value);
            int millis = (int) ( secs.Value * 1000.0 % 1000.0 );
            int micros = (int) ( secs.Value * 1000_000.0 % 1000.0 );

            TimeOnly time = new TimeOnly(hour.Value, mins.Value, secs_whole, millis, micros);
            return new ParseReport<TimeOnly>(time, fix_suggestion, warns, errs);
        } else {
            return new ParseReport<TimeOnly>(null, fix_suggestion, warns, errs);
        }
    }

    private static (ParsePart<int>, string, string) ParseHour(string text) {
        string warns = "";
        string errs = "";

        ParsePart<int> result = new(text, 12);

        try {
            int value = Int32.Parse(text);
            result = new ParsePart<int>(text, value);
        } catch (Exception) {
            result.TextShouldBe = "12";
            errs += "Invalid hours amount; ";
        }
    
        if (result.Value < 0) {
            errs += "Hours cannot be negative; ";
            int suggested_hour = ((result.Value % 24) + 24) % 24;
            result.TextShouldBe = suggested_hour.ToString();
        } else if (result.Value > 23) {
            errs += "Maximum hour is 23; ";
            int suggested_hour = ((result.Value % 24) + 24) % 24;
            result.TextShouldBe = suggested_hour.ToString();
        } else if (result.Text.Length > 2) {
            warns += "Hour should be at most 2 digits number; ";
            result.TextShouldBe = result.Value.ToString();
        }

        return (result, warns, errs);
    }

    private static (ParsePart<int>, string, string) ParseMinutes(string text) {
        string warns = "";
        string errs = "";

        ParsePart<int> result = new(text, 30);

        try {
            int value = Int32.Parse(text);
            result = new ParsePart<int>(text, value);
        } catch (Exception) {
            result.TextShouldBe = "30";
            errs += "Invalid minutes amount; ";
        }
    
        if (result.Value < 0) {
            errs += "Minutes cannot be negative; ";
            int suggested = ((result.Value % 60) + 60) % 60;
            result.TextShouldBe = suggested.ToString("D2");

        } else if (result.Value > 59) {
            errs += "Maximum minute is 59; ";
            int suggested = ((result.Value % 60) + 60) % 60;
            result.TextShouldBe = suggested.ToString("D2");

        } else if (result.Text.Length != 2) {
            warns += "Mintes should be a 2 digits number; ";
            result.TextShouldBe = result.Value.ToString("D2");
        }

        return (result, warns, errs);
    }

    private static (ParsePart<double>, string, string) ParseSeconds(string text) {
        string warns = "";
        string errs = "";

        ParsePart<double> result = new(text, 30.0);

        try {
            double value = Double.Parse(text);
            result = new ParsePart<double>(text, value);
        } catch (Exception) {
            result.TextShouldBe = "30";
            errs += "Invalid seconds amount; ";
        }
    
        if (result.Value < 0) {
            errs += "Seconds cannot be negative; ";
            double suggested = ((result.Value % 60) + 60) % 60;
            result.TextShouldBe = suggested.ToString("D2");

        } else if (result.Value >= 60) {
            errs += "Seconds cannot be 60 or more; ";
            double suggested = ((result.Value % 60) + 60) % 60;
            result.TextShouldBe = suggested.ToString("D2");

        } else if (result.Text.Split(".")[0].Length != 2) {
            warns += "Seconds whole part should be a 2 digits number; ";
            result.TextShouldBe = result.Value.ToString("D2");
        }

        return (result, warns, errs);
    }
}
