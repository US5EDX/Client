namespace Client.Stores
{
    public class StudentInfoStore
    {
        public string StudentId { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public uint GroupId { get; set; }

        public short SelectedYear { get; set; }
    }
}
