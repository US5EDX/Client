using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class SpecialtyInfo : ObservableObject
    {
        [JsonPropertyName("specialtyId")]
        public uint SpecialtyId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("specialtyName")]
        private string _specialtyName;

        [JsonPropertyName("facultyId")]
        public uint FacultyId { get; set; }
    }
}
