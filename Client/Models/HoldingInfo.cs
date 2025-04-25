using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class HoldingInfo : ObservableObject
    {
        [JsonPropertyName("eduYear")]
        public short EduYear { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("startDate")]
        private DateOnly _startDate;

        [ObservableProperty]
        [property: JsonPropertyName("endDate")]
        private DateOnly _endDate;
    }
}
