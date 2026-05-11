using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SupportService.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace SupportService.Services;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQConsumer(
        IConfiguration config,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMQConsumer> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeAsync(stoppingToken);
                break;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "RabbitMQ not ready, retrying in 5s...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:UserName"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest",
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var exchange = _config["RabbitMQ:Exchange"] ?? "ticket.events";  // 👈 added
        var queue = _config["RabbitMQ:Queue"] ?? "Ticket_queue";

        // 👇 Declare the exchange
        await _channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Fanout,
            durable: true);

        // Declare support's own queue
        await _channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // 👇 Bind queue to exchange
        await _channel.QueueBindAsync(
            queue: queue,
            exchange: exchange,
            routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await HandleMessageAsync(body);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing TicketCreated event");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        await _channel.BasicConsumeAsync(queue, false, consumer);

        _logger.LogInformation("✅ SupportService listening for TicketCreated events");
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task HandleMessageAsync(string body)
    {
        var evt = JsonSerializer.Deserialize<TicketCreatedEvent>(body, _jsonOptions);

        if (evt is null)
        {
            _logger.LogWarning("Invalid TicketCreatedEvent");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

        var exists = await db.TicketSnapshots
            .AnyAsync(t => t.TicketId == evt.TicketId);

        if (exists)
        {
            _logger.LogWarning("Ticket already exists in SupportService DB");
            return;
        }

        var snapshot = new TicketSnapshot
        {
            TicketId = evt.TicketId,
            UserId = evt.UserId,
            Title = evt.Title,
            Status = "open",
            CreatedAt = evt.CreatedAt
        };

        db.TicketSnapshots.Add(snapshot);
        await db.SaveChangesAsync();

        _logger.LogInformation("Stored Ticket #{TicketId} in SupportService", evt.TicketId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}