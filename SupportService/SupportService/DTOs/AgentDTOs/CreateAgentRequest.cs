namespace SupportService.DTOs.AgentDTOs
{
    public record CreateAgentRequest(
        string Name,
        string Email,
        string PhoneNumber,
        string Department
    );
}
