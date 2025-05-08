namespace Client.Models
{
    public class DisciplineRegistryInfo
    {
        public uint DisciplineId { get; set; }

        public string DisciplineCode { get; set; }

        public byte CatalogType { get; set; }

        public uint FacultyId { get; set; }

        public uint? SpecialtyId { get; set; }

        public string DisciplineName { get; set; }

        public byte EduLevel { get; set; }

        public byte Course { get; set; }

        public byte Semester { get; set; }

        public string Prerequisites { get; set; }

        public string Interest { get; set; }

        public int MaxCount { get; set; }

        public int MinCount { get; set; }

        public string Url { get; set; }

        public short Holding { get; set; }

        public bool IsYearLong { get; set; }

        public bool IsOpen { get; set; }
    }
}
