namespace SupportService.DTOs.AssignmentDTOs
{
    public record AssignmentResponse(
        int Id,
        int TicketId,
        int AgentId,
        string AgentName,
        string AssignedBy,
        DateTime AssignedAt,
        DateTime? ResolvedAt,
        bool IsActive,
        string Notes
    );
}
