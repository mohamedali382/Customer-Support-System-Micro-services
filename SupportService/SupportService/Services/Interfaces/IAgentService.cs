using SupportService.DTOs.AgentDTOs;

namespace SupportService.Services.Interfaces
{
    public interface IAgentService
    {
        Task<AgentListResponse> GetAllAsync(string? status);
        Task<AgentResponse?> GetByIdAsync(string id);
        Task<AgentResponse> CreateAsync(CreateAgentRequest request);
        Task<AgentResponse?> UpdateAsync(string id, UpdateAgentRequest request);
        Task<bool> DeleteAsync(string id);
    }
}
