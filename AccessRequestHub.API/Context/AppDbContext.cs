using AccessRequestHub.API.Models;
using Microsoft.EntityFrameworkCore;
using M = AccessRequestHub.API.Models;

namespace AccessRequestHub.API.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<M.Application> Applications { get; set; }
        public DbSet<AccessRequest> AccessRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccessRequest>()
                .HasOne(r => r.Requester)
                .WithMany(u => u.AccessRequests)
                .HasForeignKey(r => r.RequesterEmail)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccessRequest>()
                .HasOne(r => r.Application)
                .WithMany(a => a.AccessRequests)
                .HasForeignKey(r => r.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.AccessRequest)
                .WithMany(r => r.AuditLogs)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Actor)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.ActorEmail)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccessRequest>()
                .HasIndex(r => r.ClientRequestId)
                .IsUnique();

            modelBuilder.Entity<AccessRequest>()
                .Property(r => r.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Email = "alice@example.local",
                    Name = "Alice",
                    Role = "Requester",
                    ManagerEmail = "bob@example.local"
                },
                new User
                {
                    Email = "bob@example.local",
                    Name = "Bob",
                    Role = "Manager",
                    ManagerEmail = null
                },
                new User
                {
                    Email = "carol@example.local",
                    Name = "Carol",
                    Role = "SystemOwner",
                    ManagerEmail = null
                },
                new User
                {
                    Email = "dana@example.local",
                    Name = "Dana",
                    Role = "SystemOwner",
                    ManagerEmail = null
                },
                new User
                {
                    Email = "erin@example.local",
                    Name = "Erin",
                    Role = "Auditor",
                    ManagerEmail = null
                }
            );

            // Applications
            modelBuilder.Entity<M.Application>().HasData(
                new M.Application
                {
                    Id = 1,
                    Name = "CRM",
                    SystemOwnerEmail = "carol@example.local"
                },
                new M.Application
                {
                    Id = 2,
                    Name = "Finance Portal",
                    SystemOwnerEmail = "dana@example.local"
                }
            );
        }
    }
}
