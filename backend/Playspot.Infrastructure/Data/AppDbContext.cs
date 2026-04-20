using Microsoft.EntityFrameworkCore;
using Playspot.Domain.Entities;

namespace Playspot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<JoinRequest> JoinRequests => Set<JoinRequest>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<SavedEvent> SavedEvents => Set<SavedEvent>();
    public DbSet<EventReport> EventReports => Set<EventReport>();
    public DbSet<EventRating> EventRatings => Set<EventRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Event → Organizer (User)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // JoinRequest → Event
        modelBuilder.Entity<JoinRequest>()
            .HasOne(jr => jr.Event)
            .WithMany(e => e.JoinRequests)
            .HasForeignKey(jr => jr.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // JoinRequest → User
        modelBuilder.Entity<JoinRequest>()
            .HasOne(jr => jr.User)
            .WithMany(u => u.JoinRequests)
            .HasForeignKey(jr => jr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment → Event
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Comments)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment → User
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // SavedEvent → Event
        modelBuilder.Entity<SavedEvent>()
            .HasOne(s => s.Event)
            .WithMany(e => e.SavedEvents)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // SavedEvent → User
        modelBuilder.Entity<SavedEvent>()
            .HasOne(s => s.User)
            .WithMany(u => u.SavedEvents)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one save per user per event
        modelBuilder.Entity<SavedEvent>()
            .HasIndex(s => new { s.EventId, s.UserId })
            .IsUnique();

        // EventRating → Event
        modelBuilder.Entity<EventRating>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Ratings)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventRating → User
        modelBuilder.Entity<EventRating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one rating per user per event
        modelBuilder.Entity<EventRating>()
            .HasIndex(r => new { r.EventId, r.UserId })
            .IsUnique();

        // EventReport → Event
        modelBuilder.Entity<EventReport>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Reports)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventReport → User (Reporter)
        modelBuilder.Entity<EventReport>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}