using System;

namespace backend.Models
{
    public class CallRecord
    {
        public int Id { get; set; }
        public required string RoomName { get; set; }
        public string? CallerId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public string? Status { get; set; } // e.g., "Active", "Completed", "Transferred"
        public string? Summary { get; set; }
        public string? RecordingUrl { get; set; }
        
        public int? HandledByAgentId { get; set; }
        public AgentUser? HandledByAgent { get; set; }
    }
}
