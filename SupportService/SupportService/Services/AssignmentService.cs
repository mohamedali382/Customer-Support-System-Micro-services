using Microsoft.EntityFrameworkCore;
using SupportService.Data;
using SupportService.DTOs.AssignmentDTOs;
using SupportService.Entities;
using SupportService.Services.Interfaces;

namespace SupportService.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly SupportDbContext _context;
        private readonly TicketServiceClient _ticketClient;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(SupportDbContext context,
            TicketServiceClient ticketClient, ILogger<AssignmentService> logger)
        {
            _context = context;
            _ticketClient = ticketClient;
            _logger = logger;
        }

        public async Task<AssignmentResponse> AssignTicketAsync(AssignTicketRequest request)
        {
            var agent = await _context.Agents.FindAsync(request.AgentId)
                ?? throw new KeyNotFoundException($"Agent #{request.AgentId} not found.");

            // Close any existing active assignment for this ticket
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
                AssignedBy = request.AssignedBy,
                Notes = request.Notes,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Assignments.Add(assignment);

            // Mark agent as Busy if Available
            if (agent.Status == AgentStatus.Available)
            {
                agent.Status = AgentStatus.Busy;
                agent.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Ticket #{TicketId} assigned to Agent #{AgentId}", request.TicketId, request.AgentId);

            // ── HTTP call to TicketService to update assignment ───────────────────
            await _ticketClient.AssignTicketAsync(request.TicketId, request.AgentId, agent.Name, request.Priority);


            return MapAssignment(assignment, agent.Name);
        }

        public async Task<AssignmentResponse?> GetActiveAssignmentAsync(int ticketId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(a => a.TicketId == ticketId && a.IsActive);

            return assignment is null ? null : MapAssignment(assignment, assignment.Agent.Name);
        }

        public async Task<List<AssignmentResponse>> GetAgentAssignmentsAsync(int agentId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Agent)
                .Where(a => a.AgentId == agentId && a.IsActive)
                .ToListAsync();

            return assignments.Select(a => MapAssignment(a, a.Agent.Name)).ToList();
        }

        public async Task<bool> ResolveAsync(ResolveAssignmentRequest request)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(a => a.TicketId == request.TicketId && a.IsActive);

            if (assignment is null) return false;

            assignment.IsActive = false;
            assignment.ResolvedAt = DateTime.UtcNow;

            // Free up agent if no more active assignments
            var activeCount = await _context.Assignments
                .CountAsync(a => a.AgentId == assignment.AgentId && a.IsActive && a.Id != assignment.Id);

            if (activeCount == 0)
            {
                assignment.Agent.Status = AgentStatus.Available;
                assignment.Agent.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // ── HTTP: update ticket status to Resolved ────────────────────────────
            await _ticketClient.UpdateTicketStatusAsync(request.TicketId, "Resolved");


            return true;
        }

        private static AssignmentResponse MapAssignment(TicketAssignment a, string agentName) => new(
            a.Id, a.TicketId, a.AgentId, agentName,
            a.AssignedBy, a.AssignedAt, a.ResolvedAt, a.IsActive, a.Notes);
    }
}
