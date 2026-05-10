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

    public async Task CreateAgentAccountAsync(CreateIdentityAgentRequest request)
    {
        var baseUrl = _config["Services:IdentityService"] ?? "http://localhost:5037";
        var response = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"IdentityService registration failed: {response.StatusCode} - {error}");
        }
    }
}