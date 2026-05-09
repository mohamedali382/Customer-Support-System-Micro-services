using SupportService.DTOs.AgentDTOs;

namespace SupportService.Services.Interfaces
{
    public interface IAgentService
    {
        Task<AgentListResponse> GetAllAsync(string? status);
        Task<AgentResponse?> GetByIdAsync(int id);
        Task<AgentResponse> CreateAsync(CreateAgentRequest request);
        Task<AgentResponse?> UpdateAsync(int id, UpdateAgentRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
