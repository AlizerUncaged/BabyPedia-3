using System.Globalization;

namespace BabyPedia.Utilities;

public static class StringExtension
{
    public static string FirstCharToUpper(this string input) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
}