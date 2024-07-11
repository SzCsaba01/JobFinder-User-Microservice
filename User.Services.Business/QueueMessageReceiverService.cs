using User.Data.Contracts.Helpers.DTO.Message;
using User.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace User.Services.Business;
public class QueueMessageReceiverService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;

    public QueueMessageReceiverService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _queueName = _configuration["JobQueueCredentials:QueueName"];
        _exchangeName = _configuration["JobQueueCredentials:ExchangeName"];
        _routingKey = _configuration["JobQueueCredentials:RoutingKey"];

        var hostname = Environment.GetEnvironmentVariable("JOB_QUEUE_HOSTNAME") ?? _configuration["JobQueueCredentials:Hostname"];
        var port = Environment.GetEnvironmentVariable("JOB_QUEUE_PORT") ?? _configuration["JobQueueCredentials:Port"];

        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            Port = int.Parse(port),
            UserName = _configuration["JobQueueCredentials:Username"],
            Password = _configuration["JobQueueCredentials:Password"]
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

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);


        _channel.QueueBind
            (
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: _routingKey
            );

        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessMessageAsync(message);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        using var scope = _serviceProvider.CreateScope();

        var emailMessage = JsonConvert.DeserializeObject<JobEmailMessageDto>(message);

        var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        if (emailMessage is null)
        {
            return;
        }

        await _emailService.SendFeedbackEmailsAsync(emailMessage.Data);
    }
}
