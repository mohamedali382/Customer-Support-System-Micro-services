namespace SupportService.DTOs.AgentDTOs
{
    public record CreateAgentRequest(
        string UserId,
        string Name,
        string Email,
        string Password,
        string PhoneNumber,
        string Department
    );
}
