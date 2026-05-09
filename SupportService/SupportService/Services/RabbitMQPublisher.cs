using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupportService.Services;

public enum SupportEventType
{
    AgentCreated,
    AgentUpdated,
    AgentDeleted,
    AgentStatusChanged,
    TicketAssigned,
    TicketResolved
}

public class SupportEvent
{
    public SupportEventType EventType { get; set; }
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public int? TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class RabbitMQPublisher : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public RabbitMQPublisher(IConfiguration config, ILogger<RabbitMQPublisher> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                    UserName = _config["RabbitMQ:UserName"] ?? "guest",
                    Password = _config["RabbitMQ:Password"] ?? "guest",
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: _config["RabbitMQ:Queue"] ?? "Ticket_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("✅ SupportService connected to RabbitMQ.");
                return;
            }
            catch (Exception ex)
            {
                retries--;
                _logger.LogWarning("⚠️ RabbitMQ not ready. Retrying in 5s... ({Retries} left). {Message}",
                    retries, ex.Message);
                if (retries == 0) { _logger.LogError("❌ Could not connect to RabbitMQ."); return; }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }

    public async Task PublishAsync(SupportEvent supportEvent)
    {
        if (_channel is null)
        {
            _logger.LogWarning("⚠️ RabbitMQ channel not available. Event not published.");
            return;
        }
        try
        {
            var queue = _config["RabbitMQ:Queue"] ?? "support_queue";
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(supportEvent, _jsonOptions));
            await _channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
            _logger.LogInformation("✅ Published: {EventType} | Agent #{AgentId}",
                supportEvent.EventType, supportEvent.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to publish event.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}