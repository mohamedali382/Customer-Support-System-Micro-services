using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SupportService.Services;

/// <summary>
/// HTTP client that calls TicketService API directly
/// to update ticket assignment and status.
/// </summary>
public class TicketServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly RabbitMQPublisher _publisher;
    private readonly ILogger<TicketServiceClient> _logger;

    public TicketServiceClient(HttpClient httpClient, IConfiguration config,
        ILogger<TicketServiceClient> logger, RabbitMQPublisher publisher)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _publisher = publisher;
    }

    private string BaseUrl => _config["Services:TicketService"] ?? "http://localhost:5006";

    /// <summary>Update ticket status via TicketService API</summary>
    public async Task UpdateTicketStatusAsync(int ticketId, string status)
    {
        try
        {
            var url = $"{BaseUrl}/api/ticket/{ticketId}/status";
            var response = await _httpClient.PatchAsJsonAsync(url, new { Status = status, Updated = DateTime.UtcNow });

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Ticket #{TicketId} status updated to {Status}", ticketId, status);
            }
            else
                _logger.LogWarning("⚠️ Failed to update ticket #{TicketId} status. HTTP {Code}",
                    ticketId, response.StatusCode);

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not reach TicketService to update ticket #{TicketId}", ticketId);
        }
    }

    /// <summary>Update ticket assigned agent via TicketService API</summary>
    public async Task AssignTicketAsync(int ticketId, string agentId, string agentName, string priority)
    {
        try
        {
            var url = $"{BaseUrl}/api/ticket/{ticketId}/assign";
            var response = await _httpClient.PatchAsJsonAsync(url, new
            {
                AgentId = agentId,
                AgentName = agentName,
                Priority = priority,
                Updated = DateTime.UtcNow
            });

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("✅ Ticket #{TicketId} assigned to agent #{AgentId}", ticketId, agentId);
            else
                _logger.LogWarning("⚠️ Failed to assign ticket #{TicketId}. HTTP {Code}",
                    ticketId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not reach TicketService to assign ticket #{TicketId}", ticketId);
        }
    }
}