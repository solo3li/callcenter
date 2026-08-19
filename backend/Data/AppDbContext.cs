using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AgentUser> Agents { get; set; }
        public DbSet<CallRecord> Calls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Initial seed data for testing
            modelBuilder.Entity<AgentUser>().HasData(
                new AgentUser { Id = 1, Username = "admin", PasswordHash = "admin" } // Plaintext for demo, should be hashed in production
            );
        }
    }
}
