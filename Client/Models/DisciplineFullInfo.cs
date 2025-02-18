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

        [JsonPropertyName("subscribersCount")]
        public int SubscribersCount { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("holding")]
        private short _holding;

        [ObservableProperty]
        [property: JsonPropertyName("isOpen")]
        private bool _isOpen;

        [JsonPropertyName("creatorId")]
        public string CreatorId { get; set; }
    }
}
