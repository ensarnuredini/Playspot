namespace Playspot.Application.Interfaces;

public interface IReportService
{
    Task<bool> ReportEventAsync(int eventId, int userId, string reason);
}
