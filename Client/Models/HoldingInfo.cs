using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public class HoldingInfo : INotifyPropertyChanged
    {
        private DateOnly _startDate;
        private DateOnly _endDate;

        [JsonPropertyName("eduYear")]
        public short EduYear { get; set; }

        [JsonPropertyName("startDate")]
        public DateOnly StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("endDate")]
        public DateOnly EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
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
