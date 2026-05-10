namespace SupportService.DTOs.AgentDTOs
{
    public record CreateAgentRequest(
        string Name,
        string Email,
        string Password,
        string PhoneNumber,
        string Department
    );
}
