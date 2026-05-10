using System;
using TicketService.DTOs;
namespace TicketService.Services.Interfaces
{
    public interface IticketService
    {
        Task<TicketDto> CreateTicketAsync(string userId, TicketDto ticketDto);
        Task<TicketDtoResponse> GetTicketByIdAsync(int id);
        Task<bool> UpdateTicketStatusAsync(int id, string status);
        Task<bool> AssignTicketAsync(int ticketId, string agentId, string agentName, string priority);
        Task<IEnumerable<TicketDtoResponse>> GetAllTicketsAsync(string userId);

    }
}
