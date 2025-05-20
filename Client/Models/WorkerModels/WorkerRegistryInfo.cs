namespace Client.Models
{
    public class WorkerRegistryInfo
    {
        public string? WorkerId { get; set; }

        public string Email { get; set; } = null!;

        public byte Role { get; set; }

        public string FullName { get; set; } = null!;

        public uint FacultyId { get; set; }

        public string Department { get; set; } = null!;

        public string Position { get; set; } = null!;
    }
}
