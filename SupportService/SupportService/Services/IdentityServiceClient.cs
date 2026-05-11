using System.Net.Http.Json;
using SupportService.DTOs.AgentDTOs;


namespace SupportService.Services;

public class IdentityServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public IdentityServiceClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CreateAgentAccountAsync(CreateIdentityAgentRequest request)
    {
        var baseUrl = _config["Services:IdentityService"] ?? "http://localhost:5037";
        var response = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/auth/register", request);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to create identity account: {response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<CreateAgentResult>();
        return result!.Id;
    }
}