using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client.Parsers
{
    public partial class SemesterParser
    {
        [GeneratedRegex(@"Семестр\s*:\s*(.+?)(\.|;|$)", RegexOptions.IgnoreCase, "uk-UA")]
        private static partial Regex SemesterRegex();

        [GeneratedRegex(@"^(будь-який|парний,непарний)$", RegexOptions.IgnoreCase, "uk-UA")]
        private static partial Regex BothSemestersAsText();

        public static int ParseSemesterString(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return 0;

            var courseMatch = SemesterRegex().Match(rawText);

            if (!courseMatch.Success)
                return 0;

            string semesterPart = courseMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace("/", ",");

            if (BothSemestersAsText().IsMatch(semesterPart))
                return 0;

            if (semesterPart == "непарний")
                return 1;

            if (semesterPart == "парний")
                return 2;

            int semester = 0;
            var parts = semesterPart.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (Regex.IsMatch(part, "^[1-8]$"))
                {
                    var val = int.Parse(part);
                    semester |= 1 << (val % 2 == 1 ? 0 : 1);
                }
                else if (Regex.IsMatch(part, "^[1-8]-[1-8]$"))
                {
                    var range = part.Split('-').Select(int.Parse).ToArray();

                    return Math.Abs(range[1] - range[0]) != 0 ? 0 :
                        (range[0] % 2 == 1 ? 1 : 2);
                }

                if (semester == 3)
                    return 0;
            }

            return semester;
        }
    }
}
