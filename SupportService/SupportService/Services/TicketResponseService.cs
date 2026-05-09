using Microsoft.EntityFrameworkCore;
using SupportService.Data;
using SupportService.DTOs;
using SupportService.Entities;
using SupportService.Services.Interfaces;

namespace SupportService.Services
{
    public class TicketResponseService : ITicketResponseService
    {
        private readonly SupportDbContext _context;

        public TicketResponseService(SupportDbContext context)
        {
            _context = context;
        }

        public async Task<TicketResponseDto> AddResponseAsync(AddTicketResponseRequest request)
        {
            var agent = await _context.Agents.FindAsync(request.AgentId)
                ?? throw new KeyNotFoundException($"Agent #{request.AgentId} not found.");

            var response = new TicketResponse
            {
                TicketId = request.TicketId,
                AgentId = request.AgentId,
                AgentName = agent.Name,
                Content = request.Content,
                Resolution = request.Resolution,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketResponses.Add(response);
            await _context.SaveChangesAsync();

            return MapResponse(response);
        }

        public async Task<List<TicketResponseDto>> GetResponsesByTicketAsync(int ticketId) =>
            await _context.TicketResponses
                .Where(r => r.TicketId == ticketId)
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapResponse(r))
                .ToListAsync();

        private static TicketResponseDto MapResponse(TicketResponse r) => new(
            r.Id, r.TicketId, r.AgentId, r.AgentName,
            r.Content, r.Resolution, r.CreatedAt);
    }
}
