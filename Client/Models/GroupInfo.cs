using System.Text.Json.Serialization;

namespace Client.Models
{
    public class GroupInfo
    {
        [JsonPropertyName("groupId")]
        public uint GroupId { get; set; }

        [JsonPropertyName("groupCode")]
        public string GroupCode { get; set; }

        [JsonPropertyName("eduLevel")]
        public byte EduLevel { get; set; }

        [JsonPropertyName("course")]
        public byte Course { get; set; }

        [JsonPropertyName("durationOfStudy")]
        public byte DurationOfStudy { get; set; }

        [JsonPropertyName("admissionYear")]
        public short AdmissionYear { get; set; }

        [JsonPropertyName("nonparsemester")]
        public byte Nonparsemester { get; set; }

        [JsonPropertyName("parsemester")]
        public byte Parsemester { get; set; }

        [JsonPropertyName("hasEnterChoise")]
        public bool HasEnterChoise { get; set; }

        [JsonPropertyName("choiceDifference")]
        public byte ChoiceDifference { get; set; }
    }
}
