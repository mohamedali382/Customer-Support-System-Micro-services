namespace SupportService.DTOs.AssignmentDTOs
{
    public record ResolveAssignmentRequest(
        int TicketId,
        string ResolvedBy
    );
}
