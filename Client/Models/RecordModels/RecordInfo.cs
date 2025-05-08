using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class RecordShortInfo : ObservableObject
    {
        [JsonPropertyName("recordId")]
        public uint RecordId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("chosenSemester")]
        private byte _chosenSemester;

        [ObservableProperty]
        [property: JsonPropertyName("approved")]
        private byte _approved;

        [ObservableProperty]
        [property: JsonPropertyName("disciplineId")]
        private uint _disciplineId;

        [ObservableProperty]
        [property: JsonPropertyName("disciplineCode")]
        private string _disciplineCode;

        [ObservableProperty]
        [property: JsonPropertyName("disciplineName")]
        private string _disciplineName;

        [ObservableProperty]
        [property: JsonPropertyName("isYearLong")]
        private bool _isYearLong;
    }

    public partial class RecordInfo : RecordShortInfo
    {
        [ObservableProperty]
        [property: JsonPropertyName("course")]
        private string _course;

        [ObservableProperty]
        [property: JsonPropertyName("eduLevel")]
        private byte _eduLevel;

        [ObservableProperty]
        [property: JsonPropertyName("semester")]
        private byte _semester;

        [ObservableProperty]
        [property: JsonPropertyName("subscribersCount")]
        private int _subscribersCount;

        [ObservableProperty]
        [property: JsonPropertyName("isOpen")]
        private bool _isOpen;

        public void UpdateInfo(RecordInfo record)
        {
            Approved = record.Approved;
            DisciplineId = record.DisciplineId;
            DisciplineCode = record.DisciplineCode;
            DisciplineName = record.DisciplineName;
            Course = record.Course;
            EduLevel = record.EduLevel;
            Semester = record.Semester;
            SubscribersCount = record.SubscribersCount;
            IsOpen = record.IsOpen;
        }
    }
}
