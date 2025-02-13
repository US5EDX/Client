using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Models
{
    public class GroupShortInfo
    {
        [JsonPropertyName("groupId")]
        public uint GroupId { get; set; }

        [JsonPropertyName("groupCode")]
        public string GroupCode { get; set; }
    }
}
