using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class FacultyInfo : ObservableObject
    {
        [JsonPropertyName("facultyId")]
        public uint FacultyId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("facultyName")]
        private string _facultyName;
    }
}
