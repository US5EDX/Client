using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Models
{
    public class FacultyInfo
    {
        [JsonPropertyName("facultyId")]
        public uint FacultyId { get; set; }

        [JsonPropertyName("facultyName")]
        public string FacultyName { get; set; }
    }
}
