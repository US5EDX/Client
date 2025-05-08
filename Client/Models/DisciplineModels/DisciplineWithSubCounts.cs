using System.Text.Json.Serialization;

namespace Client.Models
{
    public class DisciplineWithSubCounts : DisciplineFullInfo
    {
        [JsonPropertyName("nonparsemesterCount")]
        public int NonparsemesterCount { get; set; }

        [JsonPropertyName("parsemesterCount")]
        public int ParsemesterCount { get; set; }

        public DisciplineWithSubCounts() : base() { }

        public DisciplineWithSubCounts(DisciplineFullInfo disciplineFullInfo) : base(disciplineFullInfo) { }

        public void UpdateInfo(in DisciplineFullInfo? discipline)
        {
            if (discipline == null)
                return;

            DisciplineCode = discipline.DisciplineCode;
            DisciplineName = discipline.DisciplineName;
            CatalogType = discipline.CatalogType;
            Specialty = discipline.Specialty;
            EduLevel = discipline.EduLevel;
            Course = discipline.Course;
            Semester = discipline.Semester;
            Prerequisites = discipline.Prerequisites;
            Interest = discipline.Interest;
            MaxCount = discipline.MaxCount;
            MinCount = discipline.MinCount;
            Url = discipline.Url;
            IsYearLong = discipline.IsYearLong;
        }
    }
}
