using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Models
{
    public class RecordDisciplineAndStatusPair
    {
        [JsonPropertyName("codeName")]
        public string CodeName { get; set; }

        [JsonPropertyName("approved")]
        public byte Approved { get; set; }
    }
}
