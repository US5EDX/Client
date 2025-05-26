namespace Client.Models
{
    public class AuditLogInfo
    {
        public uint Id { get; set; }

        public string UserId { get; set; } = null!;

        public string? IpAddress { get; set; }

        public string ActionType { get; set; } = null!;

        public string EntityName { get; set; } = null!;

        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; }

        public string? Description { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }
    }
}
