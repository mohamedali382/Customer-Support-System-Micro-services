using ReportService.DTOs;
namespace ReportService.Services;

public class TicketReportService : IReportService
{
    private readonly TicketServiceClient _ticketClient;

    public TicketReportService(TicketServiceClient ticketClient)
    {
        _ticketClient = ticketClient;
    }

    public async Task<TicketReportDto> GetReportAsync(DateTime? from, DateTime? to)
    {
        var tickets = await _ticketClient.GetResolvedTicketsAsync(from, to);

        var avgResolution = tickets.Any(t => t.ResolvedAt.HasValue)
            ? tickets
                .Where(t => t.ResolvedAt.HasValue)
                .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var agentPerformance = tickets
            .Where(t => t.AgentId != null && t.ResolvedAt.HasValue)
            .GroupBy(t => new { t.AgentId, t.AgentName })
            .Select(g => new AgentPerformanceDto
            {
                AgentId = g.Key.AgentId!,
                AgentName = g.Key.AgentName ?? "Unknown",
                ResolvedCount = g.Count(),
                AverageResolutionHours = Math.Round(
                    g.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 2)
            })
            .OrderByDescending(a => a.ResolvedCount)
            .ToList();

        var dailyTickets = tickets
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new DailyTicketDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new TicketReportDto
        {
            TotalTickets = tickets.Count,
            ResolvedTickets = tickets.Count,
            AverageResolutionHours = Math.Round(avgResolution, 2),
            AgentPerformance = agentPerformance,
            DailyTickets = dailyTickets
        };
    }
}