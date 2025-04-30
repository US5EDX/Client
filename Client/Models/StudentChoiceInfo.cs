using System.Text.Json.Serialization;

namespace Client.Models
{
    public class StudentChoiceInfo : DisplayInfo
    {
        [JsonPropertyName("holding")]
        public short Holding { get; set; }

        [JsonPropertyName("semester")]
        public byte Semester { get; set; }

        public DisplayInfo GetDisplayInfo()
        {
            return new DisplayInfo
            {
                Approved = Approved,
                DisciplineCode = DisciplineCode,
                DisciplineName = DisciplineName
            };
        }
    }

    public class DisplayInfo
    {
        [JsonPropertyName("approved")]
        public bool Approved { get; set; }

        [JsonPropertyName("disciplineCode")]
        public string DisciplineCode { get; set; }

        [JsonPropertyName("disciplineName")]
        public string DisciplineName { get; set; }
    }

    public class SemesterChoicesViewModel
    {
        public byte Semester { get; set; }
        public List<DisplayInfo> Choices { get; set; }
    }

    public class YearChoicesViewModel
    {
        public short Holding { get; set; }
        public List<SemesterChoicesViewModel> Semesters { get; set; }

        public string YearLabel => $"{Holding} / {Holding + 1} н.р.";
    }
}
