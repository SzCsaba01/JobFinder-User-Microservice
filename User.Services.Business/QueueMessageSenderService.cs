using User.Services.Contracts;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace User.Services.Business;
public class QueueMessageSenderService : IQueueMessageSenderService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;

    public QueueMessageSenderService(IConfiguration configuration)
    {
        _configuration = configuration;
        _queueName = _configuration["UserQueueCredentials:QueueName"];
        _exchangeName = _configuration["UserQueueCredentials:ExchangeName"];
        _routingKey = _configuration["UserQueueCredentials:RoutingKey"];

        var hostname = Environment.GetEnvironmentVariable("USER_QUEUE_HOSTNAME") ?? _configuration["UserQueueCredentials:Hostname"];

        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            Port = int.Parse(_configuration["UserQueueCredentials:Port"]),
            UserName = _configuration["UserQueueCredentials:Username"],
            Password = _configuration["UserQueueCredentials:Password"]
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare
            (
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
       
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
    }

    public async Task SendMessageAsync(string message)
    {
        await Task.Run(() =>
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish
                (
                    exchange: _exchangeName,
                    routingKey: _routingKey,
                    basicProperties: null,
                    body: body
                );
        });
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
