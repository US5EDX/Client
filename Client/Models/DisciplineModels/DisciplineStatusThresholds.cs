using System.Text.Json.Serialization;

namespace Client.Models
{
    public class DisciplineStatusThresholds
    {
        [JsonPropertyName("notEnough")]
        public int NotEnough { get; set; }

        [JsonPropertyName("partiallyFilled")]
        public int PartiallyFilled { get; set; }
    }
}
