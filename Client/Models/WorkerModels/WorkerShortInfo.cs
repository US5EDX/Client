using System.Text.Json.Serialization;

namespace Client.Models
{
    public class WorkerShortInfo
    {
        [JsonPropertyName("workerId")]
        public string WorkerId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }
    }
}
