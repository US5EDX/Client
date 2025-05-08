namespace Client.Models
{
    public class GroupRegistryInfo
    {
        public uint? GroupId { get; set; }

        public string GroupCode { get; set; }

        public uint SpecialtyId { get; set; }

        public byte EduLevel { get; set; }

        public byte DurationOfStudy { get; set; }

        public short AdmissionYear { get; set; }

        public byte Nonparsemester { get; set; }

        public byte Parsemester { get; set; }

        public bool HasEnterChoise { get; set; }

        public byte ChoiceDifference { get; set; }

        public string? CuratorId { get; set; }
    }
}
