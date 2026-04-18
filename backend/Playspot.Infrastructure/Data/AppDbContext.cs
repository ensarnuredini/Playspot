using Microsoft.EntityFrameworkCore;
using Playspot.Domain.Entities;

namespace Playspot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<JoinRequest> JoinRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>()
            .HasMany(u => u.OrganisedEvents)
            .WithOne(e => e.Organiser)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.JoinRequests)
            .WithOne(jr => jr.User)
            .HasForeignKey(jr => jr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event configuration
        modelBuilder.Entity<Event>()
            .HasMany(e => e.JoinRequests)
            .WithOne(jr => jr.Event)
            .HasForeignKey(jr => jr.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}
