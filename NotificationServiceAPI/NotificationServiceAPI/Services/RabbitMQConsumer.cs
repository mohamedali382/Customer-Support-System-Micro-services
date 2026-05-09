using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Contracts;
using NotificationServiceAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationServiceAPI.DTOs;
using NotificationServiceAPI.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NotificationServiceAPI.Services;

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
        await InitializeAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeAsync(CancellationToken stoppingToken)
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

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                var queue = _config["RabbitMQ:Queue"] ?? "Ticket_queue";

                await _channel.QueueDeclareAsync(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Process one message at a time
                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                // Single consumer on the single queue
                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation("📦 Raw message received: {Body}", body);
                    try
                    {
                        await HandleMessageAsync(body);

                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message");

                        // ✅ NACK — requeue so message is not lost
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                await _channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer);

                _logger.LogInformation("✅ NotificationService listening on queue: {Queue}", queue);
                return;
            }
            catch (Exception ex)
            {
                retries--;
                _logger.LogWarning("⚠️ RabbitMQ not ready. Retrying in 5s... ({Retries} left). {Message}",
                    retries, ex.Message);

                if (retries == 0)
                {
                    _logger.LogError("❌ Could not connect to RabbitMQ after all retries.");
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }



    private async Task HandleMessageAsync(string body)
    {
        using var doc = JsonDocument.Parse(body);

        var eventType = (EventType)doc.RootElement
            .GetProperty("EventType")
            .GetInt32();

        _logger.LogInformation("📨 Event received: {EventType}", eventType);

        switch (eventType)
        {
            // =========================
            // TICKET CREATED
            // =========================
            case EventType.TicketCreated:
                {
                    var evt = JsonSerializer.Deserialize<TicketCreatedEvent>(body);

                    if (evt is null) return;

                    if (!string.IsNullOrEmpty(evt.UserId))
                    {
                        await CreateNotificationAsync(
                            recipientId: evt.UserId,
                            recipientType: RecipientType.User,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketCreated,
                            description: $"Your ticket \"{evt.Title}\" was created successfully."
                        );
                    }

                    if (!string.IsNullOrEmpty(evt.AgentId))
                    {
                        await CreateNotificationAsync(
                            recipientId: evt.AgentId,
                            recipientType: RecipientType.Agent,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketCreated,
                            description: $"New ticket available: \"{evt.Title}\""
                        );
                    }

                    break;
                }

            // =========================
            // TICKET ASSIGNED
            // =========================
            case EventType.TicketAssigned:
                {
                    var evt = JsonSerializer.Deserialize<TicketAssignedEvent>(body);

                    if (evt is null) return;

                    if (!string.IsNullOrEmpty(evt.UserId))
                    {
                        await CreateNotificationAsync(
                            recipientId: evt.UserId,
                            recipientType: RecipientType.User,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketAssigned,
                            description: $"Your ticket was assigned to {evt.AgentName}."
                        );
                    }

                    if (!string.IsNullOrEmpty(evt.AgentId))
                    {
                        await CreateNotificationAsync(
                            recipientId: evt.AgentId,
                            recipientType: RecipientType.Agent,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketAssigned,
                            description: $"You have been assigned ticket \"{evt.Title}\"."
                        );
                    }

                    break;
                }

            // =========================
            // STATUS CHANGED
            // =========================
            case EventType.TicketStatusChanged:
                {
                    var evt = JsonSerializer.Deserialize<TicketStatusChangedEvent>(body);

                    if (evt is null) return;

                    if (!string.IsNullOrEmpty(evt.UserId))
                    {
                        var message = evt.Status switch
                        {
                            "Resolved" => $"Your ticket \"{evt.Title}\" has been resolved. ✅",
                            "Closed" => $"Your ticket \"{evt.Title}\" has been closed.",
                            "InProgress" => $"Your ticket \"{evt.Title}\" is now in progress.",
                            _ => $"Status changed to {evt.Status}."
                        };

                        await CreateNotificationAsync(
                            recipientId: evt.UserId,
                            recipientType: RecipientType.User,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketStatusChanged,
                            description: message
                        );
                    }

                    break;
                }

            // =========================
            // COMMENT ADDED
            // =========================
            case EventType.TicketCommentAdded:
                {
                    var evt = JsonSerializer.Deserialize<TicketCommentAddedEvent>(body);

                    if (evt is null || !evt.IsAgentReply)
                        break;

                    if (!string.IsNullOrEmpty(evt.UserId))
                    {
                        await CreateNotificationAsync(
                            recipientId: evt.UserId,
                            recipientType: RecipientType.User,
                            ticketId: evt.TicketId,
                            ticketTitle: evt.Title,
                            type: NotificationType.TicketCommentAdded,
                            description: $"Agent {evt.AuthorName} replied to your ticket."
                        );
                    }

                    break;
                }

            default:
                _logger.LogWarning("Unhandled EventType: {EventType}", eventType);
                break;
        }
    }

    //── Scoped service helper ─────────────────────────────────────────────────

    private async Task CreateNotificationAsync(
        string recipientId,
        RecipientType recipientType,
        int ticketId,
        string ticketTitle,
        NotificationType type,
        string description)
    {
        using var scope = _scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<InotificationService>();

        await service.CreateAsync(
            recipientId,
            recipientType,
            ticketId,
            ticketTitle,
            type,
            description
        );
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}