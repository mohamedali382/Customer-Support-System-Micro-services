namespace ReportService.DTOs;

public class TicketReportDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int AssignedTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResolutionHours { get; set; }
    public List<AgentPerformanceDto> AgentPerformance { get; set; } = new();
    public List<DailyTicketDto> DailyTickets { get; set; } = new();
}
