using System.Text.Json.Serialization;

namespace Client.Models
{
    public class ThresholdValue
    {
        [JsonPropertyName("notEnough")]
        public int NotEnough { get; set; }

        [JsonPropertyName("partiallyFilled")]
        public int PartiallyFilled { get; set; }
    }

    public class DisciplineStatusThresholds
    {
        [JsonPropertyName("bachelor")]
        public ThresholdValue Bachelor { get; set; } = null!;

        [JsonPropertyName("master")]
        public ThresholdValue Master { get; set; } = null!;

        [JsonPropertyName("phD")]
        public ThresholdValue PhD { get; set; } = null!;

        public ThresholdValue? GetValue(byte eduLevel) =>
            eduLevel switch
            {
                1 => Bachelor,
                2 => Master,
                3 => PhD,
                _ => null,
            };
    }
}
