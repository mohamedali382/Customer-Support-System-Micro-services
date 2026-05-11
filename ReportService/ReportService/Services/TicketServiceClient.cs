using ReportService.DTOs;
namespace ReportService.Services
{
    public class TicketServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<TicketServiceClient> _logger;

        public TicketServiceClient(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<TicketServiceClient> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        private string BaseUrl => _config["Services:TicketService"] ?? "http://localhost:5006";

        public async Task<List<TicketDto>> GetResolvedTicketsAsync(DateTime? from, DateTime? to)
        {
            try
            {
                var url = $"{BaseUrl}/api/ticket/resolved";

                var query = new List<string>();
                if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");
                if (query.Any()) url += "?" + string.Join("&", query);

                var tickets = await _httpClient.GetFromJsonAsync<List<TicketDto>>(url);
                return tickets ?? new List<TicketDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not reach TicketService");
                return new List<TicketDto>();
            }
        }
    }
}
