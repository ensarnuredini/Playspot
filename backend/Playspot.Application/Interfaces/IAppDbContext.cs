using Microsoft.EntityFrameworkCore;
using Playspot.Domain.Entities;

namespace Playspot.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<JoinRequest> JoinRequests { get; }
    DbSet<Comment> Comments { get; }
    DbSet<SavedEvent> SavedEvents { get; }
    DbSet<EventReport> EventReports { get; }
    DbSet<EventRating> EventRatings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
