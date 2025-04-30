using System.Text.Json.Serialization;

namespace Client.Models
{
    public class DisciplineInfoForStudent
    {
        [JsonPropertyName("disciplineId")]
        public uint DisciplineId { get; set; }

        [JsonPropertyName("disciplineCode")]
        public string DisciplineCode { get; set; }

        [JsonPropertyName("disciplineName")]
        public string DisciplineName { get; set; }

        [JsonPropertyName("course")]
        public string Course { get; set; }

        [JsonPropertyName("semester")]
        public byte Semester { get; set; }

        [JsonPropertyName("isYearLong")]
        public bool IsYearLong { get; set; }

        [JsonPropertyName("faculty")]
        public FacultyInfo Faculty { get; set; }
    }
}
