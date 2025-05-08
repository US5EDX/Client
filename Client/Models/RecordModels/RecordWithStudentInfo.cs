using System.Text.Json.Serialization;

namespace Client.Models
{
    public class RecordWithStudentInfo
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("facultyName")]
        public string FacultyName { get; set; }

        [JsonPropertyName("groupCode")]
        public string GroupCode { get; set; }

        [JsonPropertyName("semester")]
        public byte Semester { get; set; }

        [JsonPropertyName("approved")]
        public byte Approved { get; set; }
    }
}
