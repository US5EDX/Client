namespace Client.Models
{
    public class RecordRegistryWithoutStudent
    {
        public uint? RecordId { get; set; }

        public uint DisciplineId { get; set; }

        public byte Semester { get; set; }

        public short Holding { get; set; }
    }

    public class RecordRegistryInfo : RecordRegistryWithoutStudent
    {
        public string StudentId { get; set; }
    }
}
