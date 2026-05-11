using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IReportService
    {
        Task<TicketReportDto> GetReportAsync(DateTime? from, DateTime? to);
    }
}
