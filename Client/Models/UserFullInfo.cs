using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class UserFullInfo : ObservableObject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("email")]
        private string _email;

        [ObservableProperty]
        [property: JsonPropertyName("role")]
        private byte _role;

        [ObservableProperty]
        [property: JsonPropertyName("fullName")]
        private string _fullName;

        [ObservableProperty]
        [property: JsonPropertyName("faculty")]
        private FacultyInfo _faculty;

        [ObservableProperty]
        [property: JsonPropertyName("department")]
        private string _department;

        [ObservableProperty]
        [JsonRequired]
        [property: JsonPropertyName("position")]
        private string _position;
    }
}
