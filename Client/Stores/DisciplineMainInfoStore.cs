namespace Client.Stores
{
    public class DisciplineMainInfoStore
    {
        public uint DisciplineId { get; set; }

        public string DisciplineCode { get; set; } = null!;

        public string DisciplineName { get; set; } = null!;

        public byte Semester { get; set; }
    }
}
