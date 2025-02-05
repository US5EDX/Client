using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        [JsonPropertyName("nonparsemester")]
        public byte Nonparsemester { get; set; }

        [JsonPropertyName("parsemester")]
        public byte Parsemester { get; set; }
    }
}
