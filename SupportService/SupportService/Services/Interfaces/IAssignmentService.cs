using SupportService.DTOs.AssignmentDTOs;

namespace SupportService.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<AssignmentResponse> AssignTicketAsync(AssignTicketRequest request);
        Task<AssignmentResponse?> GetActiveAssignmentAsync(int ticketId);
        Task<List<AssignmentResponse>> GetAgentAssignmentsAsync(int agentId);
        Task<bool> ResolveAsync(ResolveAssignmentRequest request);
    }
}
