namespace SupportService.DTOs.AgentDTOs
{
    public record UpdateAgentRequest(
        string? Name,
        string? Email,
        string? PhoneNumber,
        string? Department,
        string? Status
    );
}
