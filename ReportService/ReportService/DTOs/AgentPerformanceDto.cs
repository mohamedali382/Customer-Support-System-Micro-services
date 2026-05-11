namespace ReportService.DTOs;
public class AgentPerformanceDto
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public int ResolvedCount { get; set; }
    public double AverageResolutionHours { get; set; }
}