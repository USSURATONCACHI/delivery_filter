using log4net.Config;
using log4net;
using System.Reflection;
using log4net.Util;

namespace DeliveryFilter;


public struct ParseReport {
    public DateOnly? Date;

    public string FixSuggestion;
    public string FixExplanation;
    public string Errors;

    public ParseReport(DateOnly? date, string fix_suggestion, string fix_explanation, string errors) {
        Date = date;
        FixSuggestion = fix_suggestion;
        FixExplanation = fix_explanation;
        Errors = errors;
    }
}

public class DateParser {
    public static ParseReport ParseDate(string s) {
        // If it is three integers divided with dashes

        string[] split = s.Split("-");
        if (split.Length != 3) 
            throw new ArgumentException("Not a date (1)");

        // yyyy-MM-dd
        string[] parts_text = new string[3];
        int[] parts = new int[3];
        int i = 0;

        // Check that each one is integer
        foreach (string part in split) {
            try {
                parts_text[i] = part;
                parts[i] = Int32.Parse(part);
                i += 1;
            } catch (Exception e) {
                throw new ArgumentException("Not a date (2)");
            }
        }

        string fix_explanation = "";
        string errors = "";

        // Check that year is 4 digits
        string year_text = parts_text[0];
        string year_should_be = year_text;
        int year = parts[0];
        if (year_text.Length != 4) {
            year_should_be = Math.Clamp(year, 1, 9999).ToString("D4");
            errors += "Year must be a 4 digits number; ";
        }

        if (year < 1) {
            year_should_be = "0001";
            errors += "Years before `0001` are not supported; ";
        }

        // Check that month is 2 digits
        string month_text = parts_text[1];
        string month_should_be = month_text;
        int month = parts[1];

        if (month_text.Length != 2) {
            month_should_be = month.ToString("D2");
            fix_explanation += "Month should be a 2 digits number; ";
        }
        if (month < 1) {
            month_should_be = "01";
            errors += "Month should be at least `01` (january); ";
        } else if (month > 12) {
            month_should_be = "12";
            errors += "Month should be at most `12` (december); ";
        }

        // Check that day is 2 digits
        string day_text = parts_text[2];
        string day_should_be = day_text;
        int day = parts[2];

        if (day < 1) {
            day_should_be = "01";
            errors += "Day should be at least `01`; ";
        }

        if (day_text.Length != 2) {
            day_should_be = day.ToString("D2");
            fix_explanation += "Day should be 2 characters wide; ";
        }
        if (month >= 1 && month <= 12) {
            int max_days = GetDaysInMonth(month, IsLeapYear(year));
            if (day > max_days) {
                day_should_be = max_days.ToString();
                errors += $"{GetMonthName(month)} only has {max_days} days; ";
            }
        } else if (day > 31) {
            day_should_be = "31";
            errors += "Day should be at most `31`; ";
        }

        // Return result
        string fix_suggestion = $"{year_should_be}-{month_should_be}-{day_should_be}";
        if (errors.Length == 0) {
            return new ParseReport(new DateOnly(year, month, day), fix_suggestion, fix_explanation, errors);
        } else {
            return new ParseReport(null, fix_suggestion, fix_explanation, errors);
        }
    }

    private static bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
    }

    private static int GetDaysInMonth(int month, bool isLeapYear)
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

    private static string GetMonthName(int month)
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