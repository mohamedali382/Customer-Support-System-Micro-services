namespace SupportService.DTOs
{
    public record TicketResponseDto(
        int Id,
        int TicketId,
        int AgentId,
        string AgentName,
        string Content,
        string Resolution,
        DateTime CreatedAt
    );
}
