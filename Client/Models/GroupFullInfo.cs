using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class GroupFullInfo : ObservableObject
    {
        [JsonPropertyName("groupId")]
        public uint GroupId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("groupCode")]
        private string _groupCode;

        [ObservableProperty]
        [property: JsonPropertyName("specialty")]
        private SpecialtyInfo _specialty;

        [ObservableProperty]
        [property: JsonPropertyName("eduLevel")]
        private byte _eduLevel;

        [ObservableProperty]
        [property: JsonPropertyName("course")]
        private byte _course;

        [ObservableProperty]
        [property: JsonPropertyName("durationOfStudy")]
        public byte _durationOfStudy;

        [ObservableProperty]
        [property: JsonPropertyName("admissionYear")]
        public short _admissionYear;

        [ObservableProperty]
        [property: JsonPropertyName("nonparsemester")]
        private byte _nonparsemester;

        [ObservableProperty]
        [property: JsonPropertyName("parsemester")]
        private byte _parsemester;

        [ObservableProperty]
        [property: JsonPropertyName("hasEnterChoise")]
        public bool _hasEnterChoise;

        [ObservableProperty]
        [property: JsonPropertyName("choiceDifference")]
        public byte _choiceDifference;

        [ObservableProperty]
        [property: JsonPropertyName("curatorInfo")]
        private WorkerShortInfo? _curatorInfo;

        public GroupInfo ToGroupInfo()
        {
            return new GroupInfo
            {
                GroupId = this.GroupId,
                GroupCode = this.GroupCode,
                EduLevel = this.EduLevel,
                Course = this.Course,
                DurationOfStudy = this.DurationOfStudy,
                AdmissionYear = this.AdmissionYear,
                Nonparsemester = this.Nonparsemester,
                Parsemester = this.Parsemester,
                HasEnterChoise = this.HasEnterChoise,
                ChoiceDifference = this.ChoiceDifference,
            };
        }
    }
}
