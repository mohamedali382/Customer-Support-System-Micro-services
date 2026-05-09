namespace SupportService.DTOs.AssignmentDTOs
{
    public record AssignTicketRequest(
        int TicketId,
        int AgentId,
        string Priority,
        string AssignedBy,
        string Notes = ""
    );
}
