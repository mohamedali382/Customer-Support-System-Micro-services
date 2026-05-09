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

        var queue = _config["RabbitMQ:Queue"] ?? "Ticket_queue";
        await _channel.QueueDeclareAsync(queue,
            durable: true,      // survive broker restart
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task PublishTicketCreatedAsync(TicketCreatedEvent ticketEvent)
    {
        if (_channel is null)
            throw new InvalidOperationException("Publisher not initialized. Call InitializeAsync first.");

        var queue = _config["RabbitMQ:Queue"] ?? "ticket_queue";
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ticketEvent));

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}