using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public class FacultyInfo : INotifyPropertyChanged
    {
        private string _facultyName;

        [JsonPropertyName("facultyId")]
        public uint FacultyId { get; set; }

        [JsonPropertyName("facultyName")]
        public string FacultyName
        {
            get => _facultyName;
            set
            {
                if (_facultyName != value)
                {
                    _facultyName = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
