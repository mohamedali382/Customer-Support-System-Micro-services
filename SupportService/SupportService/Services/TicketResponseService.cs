using Microsoft.EntityFrameworkCore;
using SupportService.Data;
using SupportService.DTOs;
using SupportService.Entities;
using SupportService.Services.Interfaces;
using Contracts;

namespace SupportService.Services;

public class TicketResponseService : ITicketResponseService
{
    private readonly SupportDbContext _context;
    private readonly RabbitMQPublisher _publisher;

    public TicketResponseService(
        SupportDbContext context,
        RabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<TicketResponseDto> AddResponseAsync(
        AddTicketResponseRequest request)
    {
        
        var agent = await _context.Agents.FindAsync(request.AgentId)
            ?? throw new KeyNotFoundException($"Agent {request.AgentId} not found.");

        
        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a =>
                a.TicketId == request.TicketId &&
                a.AgentId == request.AgentId &&
                a.IsActive);

        if (assignment is null)
            throw new UnauthorizedAccessException(
                $"Agent {request.AgentId} is not assigned to Ticket #{request.TicketId}.");

        // ✅ Fetch ticket snapshot to get UserId and Title
        var ticket = await _context.TicketSnapshots
            .FirstOrDefaultAsync(t => t.TicketId == request.TicketId)
            ?? throw new KeyNotFoundException($"Ticket #{request.TicketId} not found.");

        var response = new TicketResponse
        {
            TicketId = request.TicketId,
            AgentId = request.AgentId,
            AgentName = agent.Name,
            UserId = ticket.UserId,      
            Title = ticket.Title, 
            Content = request.Content,
            Resolution = request.Resolution ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketResponses.Add(response);
        await _context.SaveChangesAsync();

        await _publisher.PublishTicketCommentAsync(new TicketCommentAddedEvent
        {
            EventType = EventType.TicketCommentAdded,
            TicketId = request.TicketId,
            UserId = ticket.UserId,    
            Title = ticket.Title,       
            AgentId = request.AgentId,
            AgentName = agent.Name,
            IsAgentReply = true,
            AuthorName = agent.Name,
            CreatedAt = DateTime.UtcNow
        });

        return MapResponse(response, ticket.Title);
    }

    public async Task<List<TicketResponseDto>> GetResponsesByTicketAsync(int ticketId)
    {
        var responses = await _context.TicketResponses
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return responses.Select(r => MapResponse(r, r.Title)).ToList();
    }

    private static TicketResponseDto MapResponse(TicketResponse r, string ticketTitle) => new()
    {
        Id = r.Id,
        TicketId = r.TicketId,
        TicketTitle = ticketTitle,
        AgentId = r.AgentId,
        AgentName = r.AgentName,
        Content = r.Content,
        Resolution = r.Resolution,
        CreatedAt = r.CreatedAt
    };
}