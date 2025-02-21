using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Models
{
    public class DisciplineShortInfo
    {
        [JsonPropertyName("disciplineId")]
        public uint DisciplineId { get; set; }

        [JsonPropertyName("disciplineCodeName")]
        public string DisciplineCodeName { get; set; }
    }
}
