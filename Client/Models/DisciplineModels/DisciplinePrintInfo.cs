using System.Text.Json.Serialization;

namespace Client.Models
{
    public class DisciplinePrintInfo
    {
        [JsonPropertyName("disciplineCode")]
        public string DisciplineCode { get; set; }

        [JsonPropertyName("disciplineName")]
        public string DisciplineName { get; set; }

        [JsonPropertyName("studentsCount")]
        public int StudentsCount { get; set; }

        [JsonPropertyName("specialtyName")]
        public string? SpecialtyName { get; set; }

        [JsonPropertyName("eduLevel")]
        public byte EduLevel { get; set; }

        [JsonPropertyName("course")]
        public string Course { get; set; }

        [JsonPropertyName("semester")]
        public byte Semester { get; set; }

        [JsonPropertyName("minCount")]
        public int MinCount { get; set; }

        [JsonPropertyName("maxCount")]
        public int MaxCount { get; set; }

        [JsonPropertyName("isOpen")]
        public bool IsOpen { get; set; }

        [JsonPropertyName("colorStatus")]
        public string ColorStatus { get; set; }
    }
}
