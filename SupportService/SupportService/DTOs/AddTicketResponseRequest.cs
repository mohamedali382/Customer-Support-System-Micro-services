namespace SupportService.DTOs
{
    public record AddTicketResponseRequest(
        int TicketId,
        int AgentId,
        string Content,
        string Resolution = ""
    );
}
