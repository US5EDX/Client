using System.Text.Json.Serialization;

namespace Client.Models
{
    public class DisciplineWithSubCounts : DisciplineFullInfo
    {
        [JsonPropertyName("nonparsemesterCount")]
        public int NonparsemesterCount { get; set; }

        [JsonPropertyName("parsemesterCount")]
        public int ParsemesterCount { get; set; }

        public DisciplineWithSubCounts() : base() { }

        public DisciplineWithSubCounts(DisciplineFullInfo disciplineFullInfo) : base(disciplineFullInfo) {}
    }
}
