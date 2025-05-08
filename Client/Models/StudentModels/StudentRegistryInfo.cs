using System.Text.Json.Serialization;

namespace Client.Models
{
    public class StudentRegistryInfo
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("group")]
        public uint Group { get; set; }

        [JsonPropertyName("faculty")]
        public uint Faculty { get; set; }

        [JsonPropertyName("headman")]
        public bool Headman { get; set; }

        public StudentRecordsInfo ToStudentWithRecords()
        {
            return new StudentRecordsInfo
            {
                StudentId = StudentId,
                Email = Email,
                FullName = FullName,
                Headman = Headman,
                Nonparsemester = new List<RecordDisciplineAndStatusPair>(),
                Parsemester = new List<RecordDisciplineAndStatusPair>()
            };
        }
    }
}
