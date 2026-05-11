using Microsoft.EntityFrameworkCore;
using SupportService.Data;
using SupportService.DTOs.AgentDTOs;
using SupportService.Entities;
using SupportService.Services.Interfaces;

namespace SupportService.Services
{
    public class AgentService : IAgentService
    {
        private readonly SupportDbContext _context;
        private readonly ILogger<AgentService> _logger;
        private readonly IdentityServiceClient _identityClient;

        public AgentService(SupportDbContext context,
            ILogger<AgentService> logger, IdentityServiceClient identityClient)
        {
            _context = context;
            _logger = logger;
            _identityClient = identityClient;
        }

        public async Task<AgentListResponse> GetAllAsync(string? status)
        {
            var query = _context.Agents.AsQueryable();
            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<AgentStatus>(status, true, out var s))
                query = query.Where(a => a.Status == s);

            var agents = await query.OrderBy(a => a.Name).ToListAsync();
            return new AgentListResponse(agents.Select(MapAgent).ToList(), agents.Count);
        }

        public async Task<AgentResponse?> GetByIdAsync(string id)
        {
            var agent = await _context.Agents.FindAsync(id);
            return agent is null ? null : MapAgent(agent);
        }

        public async Task<AgentResponse?> GetByUserIdAsync(string userId)
        {
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == userId);
            return agent is null ? null : MapAgent(agent);
        }

        public async Task<AgentResponse> CreateAsync(CreateAgentRequest request)
        {
            if (await _context.Agents.AnyAsync(a => a.Email == request.Email))
                throw new InvalidOperationException($"Agent with email {request.Email} already exists.");

            var agent = new Agent
            {
                UserId = request.UserId,
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Department = request.Department,
                Status = AgentStatus.Available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _identityClient.CreateAgentAccountAsync(new CreateIdentityAgentRequest
                {
                    FullName = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    PhoneNumber = request.PhoneNumber,
                    Role = "Agent"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Identity account for {Email}", request.Email);
                throw;
            }

            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Agent #{Id} created: {Name}", agent.Id, agent.Name);
            return MapAgent(agent);
        }

        public async Task<AgentResponse?> UpdateAsync(string id, UpdateAgentRequest request)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent is null) return null;

            var previousStatus = agent.Status;

            if (!string.IsNullOrEmpty(request.Name)) agent.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Email)) agent.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) agent.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.Department)) agent.Department = request.Department;
            if (!string.IsNullOrEmpty(request.Status) &&
                Enum.TryParse<AgentStatus>(request.Status, true, out var newStatus))
                agent.Status = newStatus;

            agent.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapAgent(agent);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent is null) return false;

            _context.Agents.Remove(agent);
            await _context.SaveChangesAsync();

            return true;
        }

        private static AgentResponse MapAgent(Agent a) => new(
            a.Id, a.UserId, a.Name, a.Email, a.PhoneNumber,
            a.Department, a.Status.ToString(), a.CreatedAt, a.UpdatedAt);
    }
}
