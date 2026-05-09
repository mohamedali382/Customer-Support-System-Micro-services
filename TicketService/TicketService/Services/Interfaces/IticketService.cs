using System;
using TicketService.DTOs;
namespace TicketService.Services.Interfaces
{
    public interface IticketService
    {
        Task<TicketDto> CreateTicketAsync(string userId, TicketDto ticketDto);
        Task<TicketDtoResponse> GetTicketByIdAsync(int id, string userId);
        Task<bool> UpdateTicketStatusAsync(int id, string status);
        Task<bool> AssignTicketAsync(int ticketId, int agentId, string agentName, string priority);
        Task<IEnumerable<TicketDtoResponse>> GetAllTicketsAsync(string userId);

    }
}
