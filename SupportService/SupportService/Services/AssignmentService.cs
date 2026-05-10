using Microsoft.EntityFrameworkCore;
using SupportService.Data;
using SupportService.DTOs.AssignmentDTOs;
using SupportService.Entities;
using SupportService.Services.Interfaces;
using Contracts;

namespace SupportService.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly SupportDbContext _context;
        private readonly TicketServiceClient _ticketClient;
        private readonly ILogger<AssignmentService> _logger;
        private readonly RabbitMQPublisher _publisher;

        public AssignmentService(
            SupportDbContext context,
            TicketServiceClient ticketClient,
            ILogger<AssignmentService> logger,
            RabbitMQPublisher publisher)
        {
            _context = context;
            _ticketClient = ticketClient;
            _logger = logger;
            _publisher = publisher;
        }

        public async Task<List<TicketSnapshotDto>> GetAllAsync()
        {
            return await _context.TicketSnapshots
                .Where(t => t.Status == "Open")
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketSnapshotDto(
                    t.TicketId,
                    t.UserId,
                    t.Title,
                    t.Status,
                    t.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<TicketSnapshotDto?> GetOpenByIdAsync(int ticketId)
        {
            var ticket = await _context.TicketSnapshots
                .FirstOrDefaultAsync(t =>
                    t.TicketId == ticketId &&
                    t.Status == "Open");

            if (ticket is null)
                return null;

            return new TicketSnapshotDto(
                ticket.TicketId,
                ticket.UserId,
                ticket.Title,
                ticket.Status,
                ticket.CreatedAt
            );
        }

        public async Task<AssignmentResponse> AssignTicketAsync(AssignTicketRequest request)
        {
            var ticket = await _context.TicketSnapshots
                .FirstOrDefaultAsync(t => t.TicketId == request.TicketId);
            if (ticket is null)
                throw new KeyNotFoundException($"Ticket #{request.TicketId} not found in SupportService.");

            var agent = await _context.Agents.FindAsync(request.AgentId)
                ?? throw new KeyNotFoundException($"Agent #{request.AgentId} not found.");

            // Deactivate existing assignment if any
            var existing = await _context.Assignments
                .FirstOrDefaultAsync(a => a.TicketId == request.TicketId && a.IsActive);

            if (existing is not null)
            {
                existing.IsActive = false;
                existing.ResolvedAt = DateTime.UtcNow;
            }

            // Create new assignment
            var assignment = new TicketAssignment
            {
                TicketId = request.TicketId,
                AgentId = request.AgentId,
                AssignedBy = request.AssignedBy ?? "system",
                Notes = request.Notes,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Assignments.Add(assignment);

            // Update agent status
            if (agent.Status == AgentStatus.Available)
            {
                agent.Status = AgentStatus.Busy;
                agent.UpdatedAt = DateTime.UtcNow;
            }

            
            ticket.Status = "Assigned";
            _context.TicketSnapshots.Update(ticket);

            

            _logger.LogInformation(
                "✅ Ticket #{TicketId} assigned to Agent #{AgentId}",
                request.TicketId, request.AgentId);

            
            await _ticketClient.AssignTicketAsync(
                request.TicketId,
                request.AgentId,
                agent.Name,
                request.Priority);

            await _context.SaveChangesAsync();

            await _publisher.PublishStatusChangeAsync(new TicketStatusChangedEvent
            {
                TicketId = request.TicketId,
                UserId = ticket.UserId,
                Title = ticket.Title,
                PreviousStatus = "Open",
                Status = "Assigned",
                CreatedAt = DateTime.UtcNow
            });

            await _publisher.PublishAssignmentInfoAsync(new TicketAssignedEvent
            {
                TicketId = request.TicketId,
                AgentId = request.AgentId,
                AgentName = agent.Name,
                UserId = ticket.UserId,
                Title = ticket.Title,
                CreatedAt = DateTime.UtcNow
            });

            return MapAssignment(assignment, agent.Name);
        }

        public async Task<AssignmentResponse?> GetActiveAssignmentAsync(
            int ticketId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(a =>
                    a.TicketId == ticketId &&
                    a.IsActive);

            return assignment is null
                ? null
                : MapAssignment(assignment, assignment.Agent.Name);
        }

        public async Task<List<AssignmentResponse>>
            GetAgentAssignmentsAsync(string agentId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Agent)
                .Where(a =>
                    a.AgentId == agentId &&
                    a.IsActive)
                .ToListAsync();

            return assignments
                .Select(a => MapAssignment(a, a.Agent.Name))
                .ToList();
        }

        public async Task<bool> ResolveAsync(
            ResolveAssignmentRequest request)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(a =>
                    a.TicketId == request.TicketId &&
                    a.IsActive);
            var ticket = await _context.TicketSnapshots
                .FirstOrDefaultAsync(t => t.TicketId == request.TicketId);

            if (assignment is null)
                return false;

            assignment.IsActive = false;
            assignment.ResolvedAt = DateTime.UtcNow;

            // Free agent if no active tickets
            var activeCount = await _context.Assignments
                .CountAsync(a =>
                    a.AgentId == assignment.AgentId &&
                    a.IsActive &&
                    a.Id != assignment.Id);

            if (activeCount == 0)
            {
                assignment.Agent.Status = AgentStatus.Available;
                assignment.Agent.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Update TicketService
            await _ticketClient.UpdateTicketStatusAsync(
                request.TicketId,
                "Resolved");

            // Publish status changed event
            await _publisher.PublishStatusChangeAsync(
                new TicketStatusChangedEvent
                {
                    TicketId = request.TicketId,
                    Title = ticket.Title,
                    UserId = ticket.UserId,
                    Status = "Resolved",
                    CreatedAt = DateTime.UtcNow
                });

            _logger.LogInformation(
                "✅ Ticket #{TicketId} resolved.",
                request.TicketId);

            return true;
        }

        private static AssignmentResponse MapAssignment(
            TicketAssignment a,
            string agentName) => new(
                a.Id,
                a.TicketId,
                a.AgentId,
                agentName,
                a.AssignedBy,
                a.AssignedAt,
                a.ResolvedAt,
                a.IsActive,
                a.Notes);
    }
}