using System.Text.RegularExpressions;

namespace Client.Parsers
{
    public partial class CourseParser
    {
        [GeneratedRegex(@"Курс\s*:\s*(.+?)(\.|;|$)", RegexOptions.IgnoreCase, "uk-UA")]
        private static partial Regex CourseRegex();

        public static byte ParseCourseString(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return 0;

            var courseMatch = CourseRegex().Match(rawText);

            if (!courseMatch.Success)
                return 0;

            string coursePart = courseMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace(";", ",")
                .Replace(".", ",");

            var mask = 0;
            var parts = coursePart.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (Regex.IsMatch(part, "^[1-4]$"))
                {
                    var val = int.Parse(part);
                    mask |= 1 << (val - 1);
                }
                else if (Regex.IsMatch(part, "^[1-4]-[1-4]$"))
                {
                    var range = part.Split('-').Select(int.Parse).ToArray();
                    for (int i = Math.Min(range[0], range[1]); i <= Math.Max(range[0], range[1]); i++)
                        mask |= 1 << (i - 1);
                }
            }

            return (byte)mask;
        }

        public static byte ParseFormatedCourseString(string? formatedString)
        {
            if (string.IsNullOrWhiteSpace(formatedString))
                return 0;

            var courses = formatedString.Replace(" ", "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(c => Regex.IsMatch(c, @"^[1-4]$"))
                .Select(int.Parse)
                .ToArray();

            return (byte)courses.Aggregate(0, (current, course) => current | (1 << (course - 1)));
        }
    }
}
