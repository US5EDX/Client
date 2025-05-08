using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class StudentRecordsInfo : ObservableObject
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("email")]
        private string _email;

        [ObservableProperty]
        [property: JsonPropertyName("fullName")]
        private string _fullName;

        [ObservableProperty]
        [property: JsonPropertyName("headman")]
        private bool _headman;

        [JsonPropertyName("nonparsemester")]
        public List<RecordDisciplineAndStatusPair> Nonparsemester { get; set; }

        [JsonPropertyName("parsemester")]
        public List<RecordDisciplineAndStatusPair> Parsemester { get; set; }

        public void UpdateInfo(in StudentRegistryInfo student)
        {
            Email = student.Email;
            FullName = student.FullName;
            Headman = student.Headman;
        }
    }
}
