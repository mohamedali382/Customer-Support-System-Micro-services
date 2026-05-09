namespace SupportService.DTOs.AgentDTOs
{
    public record AgentListResponse(
        List<AgentResponse> Agents,
        int TotalCount
    );
}
