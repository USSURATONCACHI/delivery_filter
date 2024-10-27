namespace DeliveryFilter.Parse;

public class DateParser {
    public static ParseReport<DateOnly> ParseDate(string s) {
        // Check structure (A-B-C)
        string[] split = s.Split("-");
        if (split.Length != 3) 
            throw new ArgumentException("Not a date (1)");

        // Parse each part into number (123-456-789)
        ParsePart<int>[] parts;
        try {
            parts = split
                .Select(
                    x => new ParsePart<int>(x, Int32.Parse(x))
                ).ToArray();
        } catch (Exception) {
            throw new ArgumentException("Not a date (2)");
        }

        // Parse each part
        string warns = "";
        string errs = "";

        (string e, string w) = ("", "");

        // Year
        ParsePart<int> year = parts[0];
        (e, w) = CheckYear(ref year);
        errs += e;
        warns += w;

        // Month
        ParsePart<int> month = parts[1];
        (e, w) = CheckMonth(ref month);
        errs += e;
        warns += w;

        // Day
        ParsePart<int> day = parts[2];
        (e, w) = CheckDay(ref day, month.Value, DateUtil.IsLeapYear(year.Value));
        errs += e;
        warns += w;

        // Return result
        string fix_suggestion = $"{year.TextShouldBe}-{month.TextShouldBe}-{day.TextShouldBe}";

        if (errs.Length == 0) {
            return new ParseReport<DateOnly>(new DateOnly(year.Value, month.Value, day.Value), fix_suggestion, warns, errs);
        } else {
            return new ParseReport<DateOnly>(null, fix_suggestion, warns, errs);
        }
    }

    private static (string, string) CheckYear(ref ParsePart<int> year) {
        string errors = "";
        string warns = "";
        
        if (year.Text.Length != 4) {
            year.TextShouldBe = Math.Clamp(year.Value, 1, 9999).ToString("D4");
            errors += "Year must be a 4 digits number; ";
        }

        if (year.Value < 1) {
            year.TextShouldBe = "0001";
            errors += "Years before `0001` are not supported; ";
        }

        return (errors, warns);
    }

    private static (string, string) CheckMonth(ref ParsePart<int> month) {
        string errors = "";
        string warns = "";
        
        if (month.Text.Length != 2) {
            month.TextShouldBe = month.Value.ToString("D2");
            warns += "Month should be a 2 digits number; ";
        }
        if (month.Value < 1) {
            month.TextShouldBe = "01";
            errors += "Month should be at least `01` (january); ";
        } else if (month.Value > 12) {
            month.TextShouldBe = "12";
            errors += "Month should be at most `12` (december); ";
        }

        return (errors, warns);
    }
    private static (string, string) CheckDay(ref ParsePart<int> day, int month, bool is_leap_year) {
        string errors = "";
        string warns = "";

        if (day.Value < 1) {
            day.TextShouldBe = "01";
            errors += "Day should be at least `01`; ";
        }

        if (day.Text.Length != 2) {
            day.TextShouldBe = day.Value.ToString("D2");
            warns += "Day should be 2 characters wide; ";
        }
        if (month >= 1 && month <= 12) {
            int max_days = DateUtil.GetDaysInMonth(month, is_leap_year);
            if (day.Value > max_days) {
                day.TextShouldBe = max_days.ToString();
                errors += $"{DateUtil.GetMonthName(month)} only has {max_days} days; ";
            }
        } else if (day.Value > 31) {
            day.TextShouldBe = "31";
            errors += "Day should be at most `31`; ";
        }

        return (errors, warns);
    }
}

public class DateUtil {
    public static bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
    }

    public static int GetDaysInMonth(int month, bool isLeapYear)
    {
        return month switch
        {
            1 => 31,           // January
            2 => isLeapYear ? 29 : 28,  // February
            3 => 31,           // March
            4 => 30,           // April
            5 => 31,           // May
            6 => 30,           // June
            7 => 31,           // July
            8 => 31,           // August
            9 => 30,           // September
            10 => 31,          // October
            11 => 30,          // November
            12 => 31,          // December
            _ => throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12")
        };
    }

    public static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12")
        };
    }
}