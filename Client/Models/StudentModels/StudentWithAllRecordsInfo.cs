using System.Text.Json.Serialization;

namespace Client.Models
{
    public class StudentWithAllRecordsInfo
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("records")]
        public List<RecordDisciplineInfo> Records { get; set; }
    }

    public class RecordDisciplineInfo
    {
        [JsonPropertyName("disciplineCode")]
        public string DisciplineCode { get; set; }

        [JsonPropertyName("disciplineName")]
        public string DisciplineName { get; set; }

        [JsonPropertyName("eduYear")]
        public short EduYear { get; set; }

        [JsonPropertyName("semester")]
        public byte Semester { get; set; }
    }
}
