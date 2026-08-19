using System;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AgentUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
