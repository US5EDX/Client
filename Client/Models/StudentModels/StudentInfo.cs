using System.Text.Json.Serialization;

namespace Client.Models
{
    public class StudentInfo
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("group")]
        public GroupInfo Group { get; set; }

        [JsonPropertyName("faculty")]
        public FacultyInfo Faculty { get; set; }

        [JsonPropertyName("headman")]
        public bool Headman { get; set; }
    }
}
