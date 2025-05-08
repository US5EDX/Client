namespace Client.Converters
{
    public static class Converter
    {
        public static string ConvertCatalog(byte catalog)
        {
            return catalog switch
            {
                1 => "УВК",
                2 => "ФВК",
                _ => string.Empty,
            };
        }

        public static string ConvertEduLevel(byte eduLevel)
        {
            return eduLevel switch
            {
                1 => "Бакалавр",
                2 => "Магістр",
                3 => "PHD",
                _ => string.Empty,
            };
        }

        public static string ConvertSemester(byte semester)
        {
            return semester switch
            {
                0 => "Обидва",
                1 => "Осінній",
                2 => "Весняний",
                _ => string.Empty,
            };
        }

        public static string ConvertRole(byte role)
        {
            return role switch
            {
                2 => "Адміністратор",
                3 => "Викладач",
                4 => "Студент",
                _ => string.Empty,
            };
        }

        public static string ConvertByte(byte byteVal)
        {
            return byteVal switch
            {
                0 => "❌",
                1 => "✔️",
                2 => "❔",
                _ => string.Empty,
            };
        }

        public static string ConvertBool(bool boolVal) => boolVal ? "✔️" : "❌";

        public static string ConvertShortenedProgramme(byte value) => value == 0 ? "Ні" : $"Так ({value})";
    }
}