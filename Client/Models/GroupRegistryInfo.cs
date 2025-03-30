namespace Client.Models
{
    public class GroupRegistryInfo
    {
        public uint? GroupId { get; set; }

        public string GroupCode { get; set; }

        public uint SpecialtyId { get; set; }

        public byte EduLevel { get; set; }

        public byte Course { get; set; }

        public byte Nonparsemester { get; set; }

        public byte Parsemester { get; set; }

        public string? CuratorId { get; set; }
    }
}
