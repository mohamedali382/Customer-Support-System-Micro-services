using SupportService.DTOs;

namespace SupportService.Services.Interfaces
{
    public interface ITicketResponseService
    {
        Task<TicketResponseDto> AddResponseAsync(AddTicketResponseRequest request);
        Task<List<TicketResponseDto>> GetResponsesByTicketAsync(int ticketId);
    }
}
