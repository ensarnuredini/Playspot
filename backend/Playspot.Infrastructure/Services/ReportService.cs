using Microsoft.EntityFrameworkCore;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<bool> ReportEventAsync(int eventId, int userId, string reason)
    {
        // Check if already reported by this user
        var exists = await _db.EventReports
            .AnyAsync(r => r.EventId == eventId && r.ReporterId == userId);

        if (exists) return false;

        _db.EventReports.Add(new EventReport
        {
            EventId = eventId,
            ReporterId = userId,
            Reason = reason
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
