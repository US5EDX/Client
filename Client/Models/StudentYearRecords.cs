using System.Text.Json.Serialization;

namespace Client.Models
{
    public class StudentYearRecords
    {
        [JsonPropertyName("eduYear")]
        public short EduYear { get; set; }

        [JsonPropertyName("nonparsemester")]
        public List<RecordDisciplineAndStatusPair> Nonparsemester { get; set; }

        [JsonPropertyName("parsemester")]
        public List<RecordDisciplineAndStatusPair> Parsemester { get; set; }
    }
}
