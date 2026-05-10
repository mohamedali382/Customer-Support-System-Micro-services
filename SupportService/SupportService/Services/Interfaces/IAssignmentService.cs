using SupportService.DTOs.AssignmentDTOs;

namespace SupportService.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<List<TicketSnapshotDto>> GetAllAsync();
        Task<TicketSnapshotDto?> GetOpenByIdAsync(int ticketId);
        Task<AssignmentResponse> AssignTicketAsync(AssignTicketRequest request);
        Task<AssignmentResponse?> GetActiveAssignmentAsync(int ticketId);
        Task<List<AssignmentResponse>> GetAgentAssignmentsAsync(string agentId);
        Task<bool> ResolveAsync(ResolveAssignmentRequest request);
    }
}
