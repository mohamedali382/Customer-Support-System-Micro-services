using RabbitMQ.Client;  
using System.Text;
using System.Text.Json;
using Contracts;

namespace TicketService.Services;

public class RabbitMQPublisher : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQPublisher(IConfiguration config)
    {
        _config = config;
    }

    // Call once at startup
    public async Task InitializeAsync()
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

        var exchange = _config["RabbitMQ:Exchange"] ?? "ticket.events";

        // Only declare the exchange
        await _channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Fanout,
            durable: true);
    }

    public async Task PublishTicketCreatedAsync(TicketCreatedEvent ticketEvent)
    {
        if (_channel is null)
            throw new InvalidOperationException("Publisher not initialized. Call InitializeAsync first.");

        var exchange = _config["RabbitMQ:Exchange"] ?? "ticket.events";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ticketEvent));

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: "",   // fanout ignores routing key
            body: body);
    }
    public async Task PublishStatusChangeAsync(TicketStatusChangedEvent changeEvent)
    {
        if (_channel is null)
            throw new InvalidOperationException("Publisher not initialized. Call InitializeAsync first.");

       var exchange = _config["RabbitMQ:Exchange"] ?? "ticket.events";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(changeEvent));

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: "",   // fanout ignores routing key
            body: body);
    }
    public async Task PublishTicketAssignedAsync(TicketAssignedEvent assignedEvent)
    {
        if (_channel is null)
            throw new InvalidOperationException("Publisher not initialized. Call InitializeAsync first.");

        var exchange = _config["RabbitMQ:Exchange"] ?? "ticket.events";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(assignedEvent));

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: "",   // fanout ignores routing key
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}