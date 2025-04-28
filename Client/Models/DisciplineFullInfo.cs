using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class DisciplineFullInfo : ObservableObject
    {
        [JsonPropertyName("disciplineId")]
        public uint DisciplineId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("disciplineCode")]
        private string _disciplineCode;

        [ObservableProperty]
        [property: JsonPropertyName("catalogType")]
        private byte _catalogType;

        [JsonPropertyName("faculty")]
        public FacultyInfo Faculty { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("specialty")]
        private SpecialtyInfo? _specialty;

        [ObservableProperty]
        [property: JsonPropertyName("disciplineName")]
        private string _disciplineName;

        [ObservableProperty]
        [property: JsonPropertyName("eduLevel")]
        private byte _eduLevel;

        [ObservableProperty]
        [property: JsonPropertyName("course")]
        private string _course;

        [ObservableProperty]
        [property: JsonPropertyName("semester")]
        private byte _semester;

        [ObservableProperty]
        [property: JsonPropertyName("prerequisites")]
        private string _prerequisites;

        [ObservableProperty]
        [property: JsonPropertyName("interest")]
        private string _interest;

        [ObservableProperty]
        [property: JsonPropertyName("maxCount")]
        private int _maxCount;

        [ObservableProperty]
        [property: JsonPropertyName("minCount")]
        private int _minCount;

        [ObservableProperty]
        [property: JsonPropertyName("url")]
        private string _url;

        [ObservableProperty]
        [property: JsonPropertyName("holding")]
        private short _holding;

        [ObservableProperty]
        [property: JsonPropertyName("isYearLong")]
        private bool _isYearLong;

        [ObservableProperty]
        [property: JsonPropertyName("isOpen")]
        private bool _isOpen;

        public DisciplineFullInfo() { }

        public DisciplineFullInfo(DisciplineFullInfo disciplineFullInfo)
        {
            DisciplineId = disciplineFullInfo.DisciplineId;
            _disciplineCode = disciplineFullInfo._disciplineCode;
            _catalogType = disciplineFullInfo._catalogType;
            Faculty = disciplineFullInfo.Faculty;
            _specialty = disciplineFullInfo._specialty;
            _disciplineName = disciplineFullInfo._disciplineName;
            _eduLevel = disciplineFullInfo._eduLevel;
            _course = disciplineFullInfo._course;
            _semester = disciplineFullInfo._semester;
            _prerequisites = disciplineFullInfo._prerequisites;
            _interest = disciplineFullInfo._interest;
            _maxCount = disciplineFullInfo._maxCount;
            _minCount = disciplineFullInfo._minCount;
            _url = disciplineFullInfo._url;
            _holding = disciplineFullInfo._holding;
            _isYearLong = disciplineFullInfo._isYearLong;
            _isOpen = disciplineFullInfo._isOpen;
        }
    }
}
