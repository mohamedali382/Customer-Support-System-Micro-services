namespace SupportService.DTOs.AgentDTOs
{
    public record AgentResponse(
        string Id,
        string Name,
        string Email,
        string PhoneNumber,
        string Department,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
