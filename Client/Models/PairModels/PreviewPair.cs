namespace Client.Models
{
    public class PreviewPair
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public PreviewPair(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
